
using AutoMapper;
using Microsoft.Extensions.Logging;
using TripLink.Core.DTOs.Common;
using TripLink.Core.DTOs.Rides;
using TripLink.Core.Entities;
using TripLink.Core.Exceptions;
using TripLink.Core.Interfaces.Persistence;
using TripLink.Core.Interfaces.Repositories;
using TripLink.Core.Interfaces.Services;

namespace TripLink.Service.Services
{
    public class RideService : IRideService
    {
        private readonly IRideRepository _rideRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RideService> _logger;

        public RideService(
            IRideRepository rideRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<RideService> logger)
        {
            _rideRepository = rideRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        private static void ValidateRide(
            string originCity,
            string destinationCity,
            DateTime departureAtUtc,
            int capacity)
        {
            if (string.IsNullOrWhiteSpace(originCity) ||
                originCity.Length > 100 ||
                string.IsNullOrWhiteSpace(destinationCity) ||
                destinationCity.Length > 100)
            {
                throw new ArgumentException(
                    "Origin and destination cities must be valid.");
            }

            if (capacity <= 0)
            {
                throw new ArgumentException(
                    "Capacity must be greater than zero.");
            }

            if (departureAtUtc.Kind != DateTimeKind.Utc ||
                departureAtUtc <= DateTime.UtcNow)
            {
                throw new ArgumentException(
                    "Departure must be a future UTC date and time.");
            }
        }

        public async Task<RideResponse> CreateRideAsync(
            CreateRideRequest request,
            int ownerId,
            CancellationToken cancellationToken)
        {
            ValidateRide(
                request.OriginCity,
                request.DestinationCity,
                request.DepartureAtUtc,
                request.Capacity);

            var owner = await _userRepository.GetByIdAsync(
                ownerId, cancellationToken);

            if (owner == null)
            {
                throw new NotFoundException("User not found.");
            }

            var ride = _mapper.Map<Ride>(request);

            ride.OwnerId = ownerId;
            ride.BookedSeats = 0;
            ride.CreatedAtUtc = DateTime.UtcNow;

            await _rideRepository.AddAsync(
                ride, cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            _logger.LogInformation(
                 "Ride {RideId} created by user {OwnerId} from {Origin} to {Destination}.",
                 ride.Id,
                 ownerId,
                 ride.OriginCity,
                 ride.DestinationCity);

            return _mapper.Map<RideResponse>(ride);
        }

        public async Task<RideResponse> GetRideAsync(
            int rideId,
            CancellationToken cancellationToken)
        {
            var ride = await _rideRepository.GetByIdAsync(
                rideId, cancellationToken);

            if (ride == null)
            {
                throw new NotFoundException("Ride not found.");
            }

            return _mapper.Map<RideResponse>(ride);
        }

        public async Task<PagedResult<RideResponse>> SearchRidesAsync(
            RideSearchFilter filter,
            CancellationToken cancellationToken)
        {
            if (filter.Page < 1 ||
                filter.PageSize < 1 ||
                filter.PageSize > 100)
            {
                throw new ArgumentException(
                    "Invalid pagination values.");
            }

            var rides = await _rideRepository.SearchAsync(
                filter, cancellationToken);

            var totalCount = await _rideRepository.CountAsync(
                filter, cancellationToken);

            return new PagedResult<RideResponse>
            {
                Items = _mapper.Map<List<RideResponse>>(rides),
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task UpdateRideAsync(
            int rideId,
            int userId,
            UpdateRideRequest request,
            CancellationToken cancellationToken)
        {
            var ride = await _rideRepository.GetByIdForUpdateAsync(
                rideId, cancellationToken);

            if (ride == null)
            {
                throw new NotFoundException("Ride not found.");
            }

            if (ride.OwnerId != userId)
            {
                throw new ForbiddenException(
                    "You cannot update this ride.");
            }

            ValidateRide(
                request.OriginCity,
                request.DestinationCity,
                request.DepartureAtUtc,
                request.Capacity);

            if (request.Capacity < ride.BookedSeats)
            {
                throw new DomainConflictException(
                    "Capacity cannot be lower than booked seats.");
            }

            _mapper.Map(request, ride);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }

        public async Task<List<RideResponse>> GetMyRidesAsync(
    int ownerId,
    CancellationToken cancellationToken)
        {
            var rides = await _rideRepository.GetByOwnerIdAsync(
                ownerId,
                cancellationToken);

            return _mapper.Map<List<RideResponse>>(rides);
        }

        public async Task DeleteRideAsync(
            int rideId,
            int userId,
            CancellationToken cancellationToken)
        {
            var ride = await _rideRepository.GetByIdForUpdateAsync(
                rideId, cancellationToken);

            if (ride == null)
            {
                throw new NotFoundException("Ride not found.");
            }

            if (ride.OwnerId != userId)
            {
                throw new ForbiddenException(
                    "You cannot delete this ride.");
            }

            if (ride.BookedSeats > 0)
            {
                throw new DomainConflictException(
                    "Cannot delete a ride with registrations.");
            }

            _rideRepository.Remove(ride);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }
    }
}
