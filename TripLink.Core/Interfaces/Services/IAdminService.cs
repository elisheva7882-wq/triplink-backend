using TripLink.Core.DTOs.Admin;

namespace TripLink.Core.Interfaces.Services
{
    public interface IAdminService
    {
        Task<List<AdminUserResponse>> GetUsersAsync(
            CancellationToken cancellationToken);

        Task<AdminUserResponse> GetUserAsync(
            int userId,
            CancellationToken cancellationToken);

        Task DeleteRideAsync(
    int rideId,
    CancellationToken cancellationToken);
    }
}