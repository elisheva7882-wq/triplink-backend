
using TripLink.Core.DTOs.Common;
using TripLink.Core.DTOs.Rides;

namespace TripLink.Core.Interfaces.Services
{
    public interface IRideService
    {
        Task<RideResponse> CreateRideAsync(
            CreateRideRequest request,
            int ownerId,
            CancellationToken cancellationToken);

        Task<RideResponse> GetRideAsync(
            int rideId,
            CancellationToken cancellationToken);

        Task<PagedResult<RideResponse>> SearchRidesAsync(
            RideSearchFilter filter,
            CancellationToken cancellationToken);

        Task UpdateRideAsync(
            int rideId,
            int userId,
            UpdateRideRequest request,
            CancellationToken cancellationToken);
        Task<List<RideResponse>> GetMyRidesAsync(
    int ownerId,
    CancellationToken cancellationToken);

        Task DeleteRideAsync(
            int rideId,
            int userId,
            CancellationToken cancellationToken);
    }
}
