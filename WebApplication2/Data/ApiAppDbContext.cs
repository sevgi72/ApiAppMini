using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using WebApplication2.Models;

namespace WebApplication2.Data
{
    public class ApiAppDbContext:DbContext
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Organizer> Organizers { get; set; }

        public ApiAppDbContext(DbContextOptions<ApiAppDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApiAppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
        
    }
}
