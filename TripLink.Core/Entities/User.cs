using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TripLink.Core.Enums;

namespace TripLink.Core.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public ICollection<Ride> OwnedRides { get; set; } = new List<Ride>();

        public ICollection<RideRegistration> Registrations { get; set; }
            = new List<RideRegistration>();
    }
}