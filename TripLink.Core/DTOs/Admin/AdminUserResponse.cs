using TripLink.Core.Enums;

namespace TripLink.Core.DTOs.Admin
{
    public class AdminUserResponse
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}