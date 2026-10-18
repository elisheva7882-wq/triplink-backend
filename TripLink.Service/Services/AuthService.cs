using AutoMapper;
using TripLink.Core.DTOs.Auth;
using TripLink.Core.Entities;
using TripLink.Core.Enums;
using TripLink.Core.Exceptions;
using TripLink.Core.Interfaces.Persistence;
using TripLink.Core.Interfaces.Repositories;
using TripLink.Core.Interfaces.Security;
using TripLink.Core.Interfaces.Services;


namespace TripLink.Service.Services
{
    
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public AuthService(
            IUserRepository userRepository,
            IPasswordHashService passwordHashService,
            IJwtTokenService jwtTokenService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _passwordHashService = passwordHashService;
            _jwtTokenService = jwtTokenService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<User> RegisterUserAsync(
            string email,
            string password,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException(
                    "Email is required.");
            }

            if (string.IsNullOrWhiteSpace(password) ||
                password.Length < 8)
            {
                throw new ArgumentException(
                    "Password must contain at least 8 characters.");
            }

            email = email.Trim().ToLowerInvariant();

            var existingUser = await _userRepository
                .GetByEmailAsync(email, cancellationToken);

            if (existingUser != null)
            {
                throw new DomainConflictException(
                    "Email is already registered.");
            }

            var user = new User
            {
                Email = email,
                PasswordHash = _passwordHashService.Hash(password),
                Role = UserRole.User,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _userRepository.AddAsync(
                user, cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return user;
        }


        public async Task<AuthResponse> LoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            email = email.Trim().ToLowerInvariant();

            var user = await _userRepository.GetByEmailAsync(
                email, cancellationToken);

            if (user == null)
            {
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            bool isPasswordValid = _passwordHashService.Verify(
                password, user.PasswordHash);

            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            var response = _mapper.Map<AuthResponse>(user);

            response.Token = _jwtTokenService.CreateToken(user);

            return response;
        }

    }
}
