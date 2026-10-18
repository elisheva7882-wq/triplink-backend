
using TripLink.Core.Entities;
using TripLink.Core.Exceptions;
using TripLink.Core.Interfaces.Persistence;
using TripLink.Core.Interfaces.Repositories;
using TripLink.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace TripLink.Service.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IRideRepository _rideRepository;
        private readonly IRideRegistrationRepository _registrationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RegistrationService> _logger;

        public RegistrationService(
            IRideRepository rideRepository,
            IRideRegistrationRepository registrationRepository,
            IUnitOfWork unitOfWork,
            ILogger<RegistrationService> logger)
        {
            _rideRepository = rideRepository;
            _registrationRepository = registrationRepository;
            _unitOfWork = unitOfWork;
            _logger= logger;
        }

        public async Task<RideRegistration> RegisterAsync(
            int rideId,
            int passengerId,
            CancellationToken cancellationToken)
        {
            await using var transaction =
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var ride = await _rideRepository.GetByIdForUpdateAsync(
                    rideId, cancellationToken);

                if (ride == null)
                {
                    throw new NotFoundException("Ride not found.");
                }

                if (ride.OwnerId == passengerId)
                {
                    throw new DomainConflictException(
                        "You cannot register for your own ride.");
                }

                if (ride.DepartureAtUtc <= DateTime.UtcNow)
                {
                    throw new DomainConflictException(
                        "The ride has already departed.");
                }

                bool alreadyRegistered =
                    await _registrationRepository.ExistsAsync(
                        rideId, passengerId, cancellationToken);

                if (alreadyRegistered)
                {
                    throw new DomainConflictException(
                        "You are already registered for this ride.");
                }

                if (ride.BookedSeats >= ride.Capacity)
                {
                    throw new DomainConflictException(
                        "No available seats.");
                }               

                var registration = new RideRegistration
                {
                    RideId = rideId,
                    PassengerId = passengerId,
                    RegisteredAtUtc = DateTime.UtcNow
                };

                await _registrationRepository.AddAsync(
                    registration, cancellationToken);

                ride.BookedSeats++;

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
    "User {PassengerId} registered for ride {RideId}. Booked seats: {BookedSeats}/{Capacity}.",
    passengerId,
    rideId,
    ride.BookedSeats,
    ride.Capacity);

                return registration;
            }
            catch
            {
                await transaction.RollbackAsync(
                    CancellationToken.None);

                throw;
            }
        }

        public async Task CancelAsync(
            int rideId,
            int passengerId,
            CancellationToken cancellationToken)
        {
            await using var transaction =
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var ride = await _rideRepository.GetByIdForUpdateAsync(
                    rideId, cancellationToken);

                if (ride == null)
                {
                    throw new NotFoundException("Ride not found.");
                }

                var registration =
                    await _registrationRepository.GetAsync(
                        rideId, passengerId, cancellationToken);

                if (registration == null)
                {
                    throw new NotFoundException(
                        "Registration not found.");
                }

                if (ride.DepartureAtUtc <= DateTime.UtcNow)
                {
                    throw new DomainConflictException(
                        "Cannot cancel after departure.");
                }

                _registrationRepository.Remove(registration);

                ride.BookedSeats--;

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
    "User {PassengerId} cancelled registration for ride {RideId}. Booked seats: {BookedSeats}/{Capacity}.",
    passengerId,
    rideId,
    ride.BookedSeats,
    ride.Capacity);
            }
            catch
            {
                await transaction.RollbackAsync(
                    CancellationToken.None);

                throw;
            }
        }

        public async Task<List<RideRegistration>> GetByPassengerAsync(
            int passengerId,
            CancellationToken cancellationToken)
        {
            return await _registrationRepository.ListByPassengerAsync(
                passengerId, cancellationToken);
        }

        public async Task<List<RidePassengerResponse>> GetRidePassengersAsync(
    int rideId,
    int ownerId,
    CancellationToken cancellationToken)
        {
            var ride = await _rideRepository.GetByIdAsync(
                rideId,
                cancellationToken);

            if (ride is null)
            {
                throw new NotFoundException("Ride was not found.");
            }

            if (ride.OwnerId != ownerId)
            {
                throw new ForbiddenException(
                    "Only the ride owner can view the passengers.");
            }

            var registrations =
                await _registrationRepository.GetByRideIdAsync(
                    rideId,
                    cancellationToken);

            return registrations
                .Select(r => new RidePassengerResponse
                {
                    PassengerId = r.PassengerId,
                    Email = r.Passenger.Email,
                    RegisteredAtUtc = r.RegisteredAtUtc
                })
                .ToList();
        }

        public async Task RemovePassengerAsync(
        int rideId,
        int passengerId,
        int ownerId,
        CancellationToken cancellationToken)
        {
            await using var transaction =
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var ride = await _rideRepository.GetByIdForUpdateAsync(
                    rideId,
                    cancellationToken);

                if (ride == null)
                {
                    throw new NotFoundException("Ride not found.");
                }

                if (ride.OwnerId != ownerId)
                {
                    throw new ForbiddenException(
                        "Only the ride owner can remove passengers.");
                }

                var registration =
                    await _registrationRepository.GetAsync(
                        rideId,
                        passengerId,
                        cancellationToken);

                if (registration == null)
                {
                    throw new NotFoundException(
                        "Registration not found.");
                }

                if (ride.DepartureAtUtc <= DateTime.UtcNow)
                {
                    throw new DomainConflictException(
                        "Cannot remove passenger after departure.");
                }

                _registrationRepository.Remove(registration);

                ride.BookedSeats--;

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

                await transaction.CommitAsync(
                    cancellationToken);

                _logger.LogInformation(
                    "Owner {OwnerId} removed passenger {PassengerId} from ride {RideId}. Booked seats: {BookedSeats}/{Capacity}.",
                    ownerId,
                    passengerId,
                    rideId,
                    ride.BookedSeats,
                    ride.Capacity);
            }
            catch
            {
                await transaction.RollbackAsync(
                    CancellationToken.None);

                throw;
            }
        }
    }
}
