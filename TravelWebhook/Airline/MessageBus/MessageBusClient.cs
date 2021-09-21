using Airline.Models.Dto;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Airline.MessageBus
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; }
        public int Port { get; set; }
    }
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IOptions<RabbitMqSettings> _rabbitmqConfiguration;
        public string HostName = "";
        public int Port = 0;

        public MessageBusClient(IOptions<RabbitMqSettings> rabbitmqConfiguration)
        {
            _rabbitmqConfiguration = rabbitmqConfiguration;
            HostName = _rabbitmqConfiguration.Value.HostName;
            Port = _rabbitmqConfiguration.Value.Port;
        }

        public void SendMessage(NotificationMessageDto notificationMessageDto)
        {
            var factory = new ConnectionFactory() { HostName = HostName, Port = Port };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);

                var message = JsonSerializer.Serialize(notificationMessageDto);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "trigger",
                                    routingKey: "",
                                    basicProperties: null,
                                    body: body);
                Console.WriteLine("--> Message Published on Message Bus");
            }
        }
    }

}
