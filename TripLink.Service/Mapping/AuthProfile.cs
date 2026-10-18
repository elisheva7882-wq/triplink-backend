
using AutoMapper;
using TripLink.Core.DTOs.Auth;
using TripLink.Core.Entities;

namespace TripLink.Service.Mapping
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<User, AuthResponse>()
                .ForMember(
                    dest => dest.UserId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(
                    dest => dest.Role,
                    opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(
                    dest => dest.Token,
                    opt => opt.Ignore());
        }
    }
}
