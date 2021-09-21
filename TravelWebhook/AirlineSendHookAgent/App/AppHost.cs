using AirlineSendAgent.ApplicationDbContext;
using AirlineSendAgent.Client;
using AirlineSendAgent.Models.Dtos;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AirlineSendAgent.App
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; }
        public int Port { get; set; }
    }

    public class AppHost : IAppHost
    {
        private readonly SendAgentDbContext _context;
        private readonly IWebhookClient _webhookClient;
        private readonly IOptions<RabbitMqSettings> _rabbitmqConfiguration;
        public string HostName = "";
        public int Port = 0;

        public AppHost(SendAgentDbContext context, IWebhookClient webhookClient, IOptions<RabbitMqSettings> rabbitmqConfiguration)
        {
            _context = context;
            _webhookClient = webhookClient;

            _rabbitmqConfiguration = rabbitmqConfiguration;
            HostName = _rabbitmqConfiguration.Value.HostName;
            Port = _rabbitmqConfiguration.Value.Port;
        }

        //RabbitMQ Listener
        public void Run()
        {
            var factory = new ConnectionFactory() { HostName = HostName, Port = Port };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);

                var queueName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queue: queueName,
                    exchange: "trigger",
                    routingKey: "");

                var consumer = new EventingBasicConsumer(channel);
                Console.WriteLine("Listening on the message bus...");

                consumer.Received += async (ModuleHandle, ea) =>
                {
                    Console.WriteLine("Event is triggered!");

                    var body = ea.Body;
                    var notificationMessage = Encoding.UTF8.GetString(body.ToArray());
                    var message = JsonSerializer.Deserialize<NotificationMessageDto>(notificationMessage);

                    var webhookToSend = new FlightDetailChangePayloadDto()
                    {
                        WebhookType = message.WebhookType,
                        WebhookURI = string.Empty,
                        Secret = string.Empty,
                        Publisher = string.Empty,
                        OldPrice = message.OldPrice,
                        NewPrice = message.NewPrice,
                        FlightCode = message.FlightCode
                    };

                    //In Production environment, it is better to implement a retry policy(Polly Library)
                    //and send the webhook concurrently instead of doing it in a loop
                    foreach (var whs in _context.webhookSubscriptions.Where(subs => subs.WebhookType.Equals(message.WebhookType)))
                    {
                        webhookToSend.WebhookURI = whs.WebhookURI;
                        webhookToSend.Secret = whs.Secret;
                        webhookToSend.Publisher = whs.WebhookPublisher;

                        await _webhookClient.SendWebhookNotification(webhookToSend);
                    }

                };

                channel.BasicConsume(queue: queueName,
                        autoAck: true,
                        consumer: consumer);

                Console.ReadLine();

            }
        }
    }
}
