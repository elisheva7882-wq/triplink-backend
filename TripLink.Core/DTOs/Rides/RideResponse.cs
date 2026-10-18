
namespace TripLink.Core.DTOs.Rides
{
    public class RideResponse
    {
        public int Id { get; set; }

        public int OwnerId { get; set; }

        public string OriginCity { get; set; } = string.Empty;

        public string DestinationCity { get; set; } = string.Empty;

        public DateTime DepartureAtUtc { get; set; }

        public int Capacity { get; set; }

        public int BookedSeats { get; set; }

        public int AvailableSeats { get; set; }
    }
}
