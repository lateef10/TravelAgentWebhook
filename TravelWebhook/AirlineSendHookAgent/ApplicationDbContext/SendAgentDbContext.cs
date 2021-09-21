using AirlineSendAgent.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirlineSendAgent.ApplicationDbContext
{
    public class SendAgentDbContext : DbContext
    {
        public SendAgentDbContext(DbContextOptions<SendAgentDbContext> opt) : base(opt)
        {

        }

        public DbSet<WebhookSubscription> webhookSubscriptions { get; set; }
    }
}
