using TripLink.Core.Entities;

namespace TripLink.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken);

        Task<List<User>> GetAllAsync(
    CancellationToken cancellationToken);

        Task AddAsync(
            User user,
            CancellationToken cancellationToken);
    }
}