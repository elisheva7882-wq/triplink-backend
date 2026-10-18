
using System.ComponentModel.DataAnnotations;

namespace TripLink.Core.DTOs.Rides
{
    public class CreateRideRequest
    {
        [Required]
        [StringLength(100)]
        public string OriginCity { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DestinationCity { get; set; } = string.Empty;

        [Required]
        public DateTime DepartureAtUtc { get; set; }

        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }
    }
}
