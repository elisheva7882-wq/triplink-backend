using TripLink.Core.DTOs.Admin;
using TripLink.Core.Exceptions;
using TripLink.Core.Interfaces.Persistence;
using TripLink.Core.Interfaces.Repositories;
using TripLink.Core.Interfaces.Services;

namespace TripLink.Service.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRideRepository _rideRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AdminService(
    IUserRepository userRepository,
    IRideRepository rideRepository,
    IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _rideRepository = rideRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<AdminUserResponse>> GetUsersAsync(
            CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllAsync(
                cancellationToken);

            return users.Select(user => new AdminUserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                CreatedAtUtc = user.CreatedAtUtc
            }).ToList();
        }

        public async Task<AdminUserResponse> GetUserAsync(
            int userId,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(
                userId,
                cancellationToken);

            if (user == null)
            {
                throw new NotFoundException(
                    "User not found.");
            }

            return new AdminUserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                CreatedAtUtc = user.CreatedAtUtc
            };
        }
        public async Task DeleteRideAsync(
    int rideId,
    CancellationToken cancellationToken)
        {
            var ride = await _rideRepository.GetByIdForUpdateAsync(
                rideId,
                cancellationToken);

            if (ride == null)
            {
                throw new NotFoundException(
                    "Ride not found.");
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