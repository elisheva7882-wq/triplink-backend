using TripLink.Core.Entities;

namespace TripLink.Core.Interfaces.Services
{
    public interface IRegistrationService
    {
        Task<RideRegistration> RegisterAsync(
            int rideId,
            int passengerId,
            CancellationToken cancellationToken);

        Task CancelAsync(
            int rideId,
            int passengerId,
            CancellationToken cancellationToken);

        Task<List<RideRegistration>> GetByPassengerAsync(
            int passengerId,
            CancellationToken cancellationToken);

        Task<List<RidePassengerResponse>> GetRidePassengersAsync(
    int rideId,
    int ownerId,
    CancellationToken cancellationToken);

        Task RemovePassengerAsync(
    int rideId,
    int passengerId,
    int ownerId,
    CancellationToken cancellationToken);
    }
}