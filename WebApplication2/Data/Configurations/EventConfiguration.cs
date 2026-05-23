using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication2.Models;

namespace WebApplication2.Data.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(150);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.Location)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne(e => e.Organizer)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganizerId);
        }
    }
}
