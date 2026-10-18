using Microsoft.EntityFrameworkCore;
using TripLink.Core.Entities;

namespace TripLink.Data.Context
{
    public class TripLinkDbContext : DbContext
    {
        public TripLinkDbContext(DbContextOptions<TripLinkDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Ride> Rides => Set<Ride>();
        public DbSet<RideRegistration> RideRegistrations => Set<RideRegistration>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(TripLinkDbContext).Assembly);
        }
    }
}