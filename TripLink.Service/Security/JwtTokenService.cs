
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using TripLink.Core.Entities;
using TripLink.Core.Interfaces.Security;

namespace TripLink.Service.Security
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtTokenService(
            string base64Key,
            string issuer,
            string audience)
        {
            byte[] keyBytes = Convert.FromBase64String(base64Key);

            if (keyBytes.Length < 32)
            {
                throw new ArgumentException(
                    "JWT signing key must be at least 32 bytes.");
            }

            _key = new SymmetricSecurityKey(keyBytes);
            _issuer = issuer;
            _audience = audience;
        }

        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Role,
                    user.Role.ToString()),

                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            var credentials = new SigningCredentials(
                _key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}
