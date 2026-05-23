using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication2.Models;

namespace WebApplication2.Data.Configurations
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
           builder.Property(t => t.Type)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(t => t.Price)
                .HasColumnType("decimal(18,2)");
            builder.Property(t => t.QuantityAvailable)
                .IsRequired();
            builder.HasOne(t => t.Event)
                .WithMany(e => e.Tickets)
                .HasForeignKey(t => t.EventId);
        }
    }
}
