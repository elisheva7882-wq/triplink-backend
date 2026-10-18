
using System.ComponentModel.DataAnnotations;

namespace TripLink.Core.DTOs.Rides
{
    public class RideSearchFilter
    {
        [StringLength(100)]
        public string? OriginCity { get; set; }

        [StringLength(100)]
        public string? DestinationCity { get; set; }

        public DateTime? DepartureDate { get; set; }

        [StringLength(20)]
        public string SortBy { get; set; } = "departure";

        public bool SortDescending { get; set; } = false;

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }
}
