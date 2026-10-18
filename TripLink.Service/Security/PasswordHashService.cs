
using Microsoft.AspNetCore.Identity;
using TripLink.Core.Entities;
using TripLink.Core.Interfaces.Security;

namespace TripLink.Service.Security
{
    public class PasswordHashService : IPasswordHashService
    {
        private readonly PasswordHasher<User> _passwordHasher;

        public PasswordHashService()
        {
            _passwordHasher = new PasswordHasher<User>();
        }

        public string Hash(string password)
        {
            return _passwordHasher.HashPassword(
                new User(),
                password);
        }

        public bool Verify(string password, string passwordHash)
        {
            var result = _passwordHasher.VerifyHashedPassword(
                new User(),
                passwordHash,
                password);

            return result != PasswordVerificationResult.Failed;
        }
    }
}
