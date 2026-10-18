using TripLink.Core.Entities;

namespace TripLink.Core.Interfaces.Security
{
    public interface IJwtTokenService
    {
        string CreateToken(User user);
    }
}