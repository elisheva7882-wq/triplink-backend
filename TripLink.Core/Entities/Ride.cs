using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripLink.Core.Entities
{
    public class Ride
    {
        public int Id { get; set; }

        public int OwnerId { get; set; }

        public string OriginCity { get; set; } = string.Empty;

        public string DestinationCity { get; set; } = string.Empty;

        public DateTime DepartureAtUtc { get; set; }

        public int Capacity { get; set; }

        public int BookedSeats { get; set; }

        public uint Version { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public User Owner { get; set; } = null!;

        public ICollection<RideRegistration> Registrations { get; set; }
            = new List<RideRegistration>();
    }
}