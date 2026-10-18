
using TripLink.Core.DTOs.Auth;
using TripLink.Core.Entities;

namespace TripLink.Core.Interfaces.Services
{
    public interface IAuthService
    {
        Task<User> RegisterUserAsync(
            string email,
            string password,
            CancellationToken cancellationToken);

        Task<AuthResponse> LoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken);
    }
}
