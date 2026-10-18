
using System.ComponentModel.DataAnnotations;

namespace TripLink.Core.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}
