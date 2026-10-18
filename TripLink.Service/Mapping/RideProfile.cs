
using AutoMapper;
using TripLink.Core.DTOs.Rides;
using TripLink.Core.Entities;

namespace TripLink.Service.Mapping
{
    public class RideProfile : Profile
    {
        public RideProfile()
        {
            CreateMap<CreateRideRequest, Ride>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.OwnerId, o => o.Ignore())
                .ForMember(d => d.BookedSeats, o => o.Ignore())
                .ForMember(d => d.Version, o => o.Ignore())
                .ForMember(d => d.CreatedAtUtc, o => o.Ignore())
                .ForMember(d => d.Owner, o => o.Ignore())
                .ForMember(d => d.Registrations, o => o.Ignore());

            CreateMap<UpdateRideRequest, Ride>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.OwnerId, o => o.Ignore())
                .ForMember(d => d.BookedSeats, o => o.Ignore())
                .ForMember(d => d.Version, o => o.Ignore())
                .ForMember(d => d.CreatedAtUtc, o => o.Ignore())
                .ForMember(d => d.Owner, o => o.Ignore())
                .ForMember(d => d.Registrations, o => o.Ignore());

            CreateMap<Ride, RideResponse>()
                .ForMember(
                    d => d.AvailableSeats,
                    o => o.MapFrom(
                        s => s.Capacity - s.BookedSeats));
        }
    }
}
