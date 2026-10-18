using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripLink.Core.Exceptions
{
    public class DomainConflictException : Exception
    {
        public DomainConflictException(string message)
            : base(message)
        {
        }
    }
}