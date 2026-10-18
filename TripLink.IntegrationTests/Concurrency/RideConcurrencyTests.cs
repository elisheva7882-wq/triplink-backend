using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TripLink.Core.Entities;
using TripLink.Core.Enums;
using TripLink.Data.Context;

namespace TripLink.IntegrationTests.Concurrency
{
    public class RideConcurrencyTests
    {
        [Fact]
        public async Task TwoDbContexts_WhenSameRideUpdated_SecondSaveThrowsConcurrencyException()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<RideConcurrencyTests>()
                .Build();

            var connectionString =
                configuration.GetConnectionString("Test")
                ?? throw new InvalidOperationException(
                    "Test connection string was not found.");

            var options =
                new DbContextOptionsBuilder<TripLinkDbContext>()
                    .UseNpgsql(connectionString)
                    .Options;

            // Prepare a clean TEST database
            await using (var setupContext =
                new TripLinkDbContext(options))
            {
                await setupContext.Database.EnsureDeletedAsync();
                await setupContext.Database.MigrateAsync();

                var user = new User
                {
                    Email = "concurrency@test.com",
                    PasswordHash = "test-only",
                    Role = UserRole.User,
                    CreatedAtUtc = DateTime.UtcNow
                };

                setupContext.Users.Add(user);
                await setupContext.SaveChangesAsync();

                var ride = new Ride
                {
                    OwnerId = user.Id,
                    OriginCity = "Jerusalem",
                    DestinationCity = "Tel Aviv",
                    DepartureAtUtc = DateTime.UtcNow.AddDays(1),
                    Capacity = 2,
                    BookedSeats = 0,
                    CreatedAtUtc = DateTime.UtcNow
                };

                setupContext.Rides.Add(ride);
                await setupContext.SaveChangesAsync();
            }

            // Two completely separate DbContexts
            await using var context1 =
                new TripLinkDbContext(options);

            await using var context2 =
                new TripLinkDbContext(options);

            var ride1 = await context1.Rides.SingleAsync();
            var ride2 = await context2.Rides.SingleAsync();

            // Both read the same version of the Ride
            ride1.BookedSeats = 1;
            ride2.BookedSeats = 1;

            // First context succeeds
            await context1.SaveChangesAsync();

            // Second context still has the old Version/xmin
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                () => context2.SaveChangesAsync());
        }
    }
}