using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripLink.Core.Entities;

namespace TripLink.Data.Configurations
{
    public class RideRegistrationConfiguration
        : IEntityTypeConfiguration<RideRegistration>
    {
        public void Configure(EntityTypeBuilder<RideRegistration> builder)
        {
            builder.HasKey(r => new
            {
                r.RideId,
                r.PassengerId
            });

            builder.Property(r => r.RegisteredAtUtc)
                .IsRequired();

            builder.HasOne(r => r.Ride)
                .WithMany(r => r.Registrations)
                .HasForeignKey(r => r.RideId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Passenger)
                .WithMany(u => u.Registrations)
                .HasForeignKey(r => r.PassengerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}