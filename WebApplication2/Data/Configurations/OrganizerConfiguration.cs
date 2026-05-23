using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication2.Models;

namespace WebApplication2.Data.Configurations
{
    public class OrganizerConfiguration : IEntityTypeConfiguration<Organizer>
    {
        public void Configure(EntityTypeBuilder<Organizer> builder)
        {
            builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(100);

            builder.Property(o => o.Email)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(o => o.Phone)
                .HasMaxLength(20);

            builder.Property(o => o.LogoUrl)
                .HasMaxLength(300);

            
            builder.HasIndex(o => o.Email)
                .IsUnique();
        }
    }
}
