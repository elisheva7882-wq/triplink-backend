
using Microsoft.EntityFrameworkCore;
using TripLink.Core.Entities;
using TripLink.Core.Enums;
using TripLink.Core.Interfaces.Security;
using TripLink.Data.Context;

namespace TripLink.Data.Seed
{
    public class DemoDataSeeder
    {
        private readonly TripLinkDbContext _context;
        private readonly IPasswordHashService _passwordHashService;

        public DemoDataSeeder(
            TripLinkDbContext context,
            IPasswordHashService passwordHashService)
        {
            _context = context;
            _passwordHashService = passwordHashService;
        }

        public async Task SeedAsync(
            string demoPassword,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(demoPassword))
            {
                throw new ArgumentException(
                    "Demo password is required.");
            }

            const string demoEmail = "driver@triplink.example";

            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == demoEmail,
                    cancellationToken);

            bool userAlreadyExists = user != null;

            if (user == null)
            {
                user = new User
                {
                    Email = demoEmail,
                    PasswordHash = _passwordHashService.Hash(demoPassword),
                    Role = UserRole.User,
                    CreatedAtUtc = DateTime.UtcNow
                };

                await _context.Users.AddAsync(
                    user, cancellationToken);
            }

            const string adminEmail = "admin@triplink.example";

            var admin = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == adminEmail,
                    cancellationToken);

            if (admin == null)
            {
                admin = new User
                {
                    Email = adminEmail,
                    PasswordHash = _passwordHashService.Hash(demoPassword),
                    Role = UserRole.Admin,
                    CreatedAtUtc = DateTime.UtcNow
                };

                await _context.Users.AddAsync(
                    admin,
                    cancellationToken);
            }

            bool rideExists = userAlreadyExists &&
                await _context.Rides.AnyAsync(
                    r => r.OwnerId == user!.Id &&
                         r.OriginCity == "Jerusalem" &&
                         r.DestinationCity == "Tel Aviv",
                    cancellationToken);

            if (!rideExists)
            {
                var ride = new Ride
                {
                    Owner = user,
                    OriginCity = "Jerusalem",
                    DestinationCity = "Tel Aviv",
                    DepartureAtUtc = DateTime.UtcNow.Date
                        .AddDays(7).AddHours(9),
                    Capacity = 3,
                    BookedSeats = 0,
                    CreatedAtUtc = DateTime.UtcNow
                };

                await _context.Rides.AddAsync(
                    ride, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
