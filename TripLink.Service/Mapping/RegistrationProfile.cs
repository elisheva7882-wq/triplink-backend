
using AutoMapper;
using TripLink.Core.DTOs.Registrations;
using TripLink.Core.Entities;

namespace TripLink.Service.Mapping
{
    public class RegistrationProfile : Profile
    {
        public RegistrationProfile()
        {
            CreateMap<RideRegistration, RegistrationResponse>();
        }
    }
}
