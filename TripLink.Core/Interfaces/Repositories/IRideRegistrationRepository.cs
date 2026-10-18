using TripLink.Core.Entities;

namespace TripLink.Core.Interfaces.Repositories
{
    public interface IRideRegistrationRepository
    {
        Task<bool> ExistsAsync(
            int rideId,
            int passengerId,
            CancellationToken cancellationToken);

        Task<RideRegistration?> GetAsync(
            int rideId,
            int passengerId,
            CancellationToken cancellationToken);

        Task AddAsync(
            RideRegistration registration,
            CancellationToken cancellationToken);

        Task<List<RideRegistration>> GetByRideIdAsync(
            int rideId,
            CancellationToken cancellationToken);

        void Remove(RideRegistration registration);

        Task<List<RideRegistration>> ListByPassengerAsync(
            int passengerId,
            CancellationToken cancellationToken);
    }
}