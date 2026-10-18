using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using TripLink.Core.Entities;
using TripLink.Core.Enums;
using TripLink.Core.Exceptions;
using TripLink.Data.Context;
using TripLink.Data.Persistence;
using TripLink.Data.Repositories;
using TripLink.Service.Services;

namespace TripLink.IntegrationTests.Concurrency
{
    public class RegistrationConcurrencyTests
    {
        [Fact]
        public async Task TwoConcurrentRegistrations_ForOneSeat_OnlyOneSucceeds()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<RegistrationConcurrencyTests>()
                .Build();

            var connectionString =
                configuration.GetConnectionString("Test")
                ?? throw new InvalidOperationException(
                    "Test connection string was not found.");

            var options =
                new DbContextOptionsBuilder<TripLinkDbContext>()
                    .UseNpgsql(connectionString)
                    .Options;

            int rideId;
            int passenger1Id;
            int passenger2Id;

            // Clean test DB + seed
            await using (var setupContext =
                new TripLinkDbContext(options))
            {
                await setupContext.Database.EnsureDeletedAsync();
                await setupContext.Database.MigrateAsync();

                var driver = new User
                {
                    Email = "driver@test.com",
                    PasswordHash = "test-only",
                    Role = UserRole.User,
                    CreatedAtUtc = DateTime.UtcNow
                };

                var passenger1 = new User
                {
                    Email = "passenger1@test.com",
                    PasswordHash = "test-only",
                    Role = UserRole.User,
                    CreatedAtUtc = DateTime.UtcNow
                };

                var passenger2 = new User
                {
                    Email = "passenger2@test.com",
                    PasswordHash = "test-only",
                    Role = UserRole.User,
                    CreatedAtUtc = DateTime.UtcNow
                };

                setupContext.Users.AddRange(
                    driver,
                    passenger1,
                    passenger2);

                await setupContext.SaveChangesAsync();

                var ride = new Ride
                {
                    OwnerId = driver.Id,
                    OriginCity = "Jerusalem",
                    DestinationCity = "Tel Aviv",
                    DepartureAtUtc = DateTime.UtcNow.AddDays(1),
                    Capacity = 1,
                    BookedSeats = 0,
                    CreatedAtUtc = DateTime.UtcNow
                };

                setupContext.Rides.Add(ride);
                await setupContext.SaveChangesAsync();

                rideId = ride.Id;
                passenger1Id = passenger1.Id;
                passenger2Id = passenger2.Id;
            }

            // Each request gets its own DbContext
            async Task<bool> TryRegisterAsync(int passengerId)
            {
                await using var context =
                    new TripLinkDbContext(options);

                var rideRepository =
                    new RideRepository(context);

                var registrationRepository =
                    new RideRegistrationRepository(context);

                var unitOfWork =
                    new UnitOfWork(context);

                var service =
                    new RegistrationService(
                        rideRepository,
                        registrationRepository,
                        unitOfWork,
                        NullLogger<RegistrationService>.Instance);

                try
                {
                    await service.RegisterAsync(
                        rideId,
                        passengerId,
                        CancellationToken.None);

                    return true;
                }
                catch (DomainConflictException)
                {
                    return false;
                }
            }

            // Act
            var results = await Task.WhenAll(
                TryRegisterAsync(passenger1Id),
                TryRegisterAsync(passenger2Id));

            // Assert
            Assert.Single(results.Where(success => success));
            Assert.Single(results.Where(success => !success));

            await using var verifyContext =
                new TripLinkDbContext(options);

            var rideAfter =
                await verifyContext.Rides
                    .SingleAsync(r => r.Id == rideId);

            var registrationsCount =
                await verifyContext.RideRegistrations
                    .CountAsync(r => r.RideId == rideId);

            Assert.Equal(1, rideAfter.BookedSeats);
            Assert.Equal(1, registrationsCount);
        }
    }
}