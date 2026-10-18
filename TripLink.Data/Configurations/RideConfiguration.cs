using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripLink.Core.Entities;

namespace TripLink.Data.Configurations
{
    public class RideConfiguration : IEntityTypeConfiguration<Ride>
    {
        public void Configure(EntityTypeBuilder<Ride> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.OriginCity)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.DestinationCity)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.DepartureAtUtc)
                .IsRequired();

            builder.Property(r => r.Capacity)
                .IsRequired();

            builder.Property(r => r.BookedSeats)
                .IsRequired();

            builder.Property(r => r.CreatedAtUtc)
                .IsRequired();

            builder.Property(r => r.Version)
                .IsRowVersion();

            builder.HasOne(r => r.Owner)
                .WithMany(u => u.OwnedRides)
                .HasForeignKey(r => r.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new
            {
                r.OriginCity,
                r.DestinationCity,
                r.DepartureAtUtc
            });
            builder.HasIndex(r => r.DepartureAtUtc);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_Rides_Capacity_Positive",
                    "\"Capacity\" > 0");

                t.HasCheckConstraint(
                    "CK_Rides_BookedSeats_Valid",
                    "\"BookedSeats\" >= 0 AND \"BookedSeats\" <= \"Capacity\"");
            });
        }
    }
}