
using TripLink.Core.DTOs.Rides;
using TripLink.Core.Entities;

namespace TripLink.Core.Interfaces.Repositories
{
    public interface IRideRepository
    {
        Task<Ride?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken);

        Task<Ride?> GetByIdForUpdateAsync(
            int id,
            CancellationToken cancellationToken);

        Task<List<Ride>> GetByOwnerIdAsync(
    int ownerId,
    CancellationToken cancellationToken);

        Task<List<Ride>> SearchAsync(
            RideSearchFilter filter,
            CancellationToken cancellationToken);

        Task<int> CountAsync(
            RideSearchFilter filter,
            CancellationToken cancellationToken);

        Task AddAsync(
            Ride ride,
            CancellationToken cancellationToken);

        void Remove(Ride ride);
    }
}
