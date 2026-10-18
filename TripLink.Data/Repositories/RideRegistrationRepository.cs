
using Microsoft.EntityFrameworkCore;
using TripLink.Core.Entities;
using TripLink.Core.Interfaces.Repositories;
using TripLink.Data.Context;

namespace TripLink.Data.Repositories
{
    public class RideRegistrationRepository
        : IRideRegistrationRepository
    {
        private readonly TripLinkDbContext _context;

        public RideRegistrationRepository(
            TripLinkDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(
            int rideId,
            int passengerId,
            CancellationToken cancellationToken)
        {
            return await _context.RideRegistrations
                .AnyAsync(
                    r => r.RideId == rideId &&
                         r.PassengerId == passengerId,
                    cancellationToken);
        }

        public async Task<RideRegistration?> GetAsync(
            int rideId,
            int passengerId,
            CancellationToken cancellationToken)
        {
            return await _context.RideRegistrations
                .FirstOrDefaultAsync(
                    r => r.RideId == rideId &&
                         r.PassengerId == passengerId,
                    cancellationToken);
        }

        public async Task AddAsync(
            RideRegistration registration,
            CancellationToken cancellationToken)
        {
            await _context.RideRegistrations.AddAsync(
                registration,
                cancellationToken);
        }

        public async Task<List<RideRegistration>> GetByRideIdAsync(
    int rideId,
    CancellationToken cancellationToken)
        {
            return await _context.RideRegistrations
                .AsNoTracking()
                .Include(r => r.Passenger)
                .Where(r => r.RideId == rideId)
                .OrderBy(r => r.RegisteredAtUtc)
                .ToListAsync(cancellationToken);
        }

        public void Remove(RideRegistration registration)
        {
            _context.RideRegistrations.Remove(registration);
        }

        public async Task<List<RideRegistration>> ListByPassengerAsync(
            int passengerId,
            CancellationToken cancellationToken)
        {
            return await _context.RideRegistrations
                .AsNoTracking()
                .Where(r => r.PassengerId == passengerId)
                .ToListAsync(cancellationToken);
        }
    }
}
