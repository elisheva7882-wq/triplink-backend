using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripLink.Core.Entities
{
    public class RideRegistration
    {
        public int RideId { get; set; }

        public int PassengerId { get; set; }

        public DateTime RegisteredAtUtc { get; set; }

        public Ride Ride { get; set; } = null!;

        public User Passenger { get; set; } = null!;
    }
}