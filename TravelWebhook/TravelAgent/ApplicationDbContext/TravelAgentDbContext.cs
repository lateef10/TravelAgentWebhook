using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelAgent.Models;

namespace TravelAgent.ApplicationDbContext
{
    public class TravelAgentDbContext : DbContext
    {
        public TravelAgentDbContext(DbContextOptions<TravelAgentDbContext> opt) : base(opt)
        {

        }

        public DbSet<WebhookSecret> SubscriptionSecrets { get; set; }
    }
}
