using Microsoft.AspNetCore.Mvc;
using TripLink.Core.DTOs.Auth;
using TripLink.Core.Interfaces.Services;

namespace TripLink.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            var user = await _authService.RegisterUserAsync(
                request.Email,
                request.Password,
                cancellationToken);

            return StatusCode(201, new
            {
                userId = user.Id,
                email = user.Email,
                role = user.Role.ToString()
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _authService.LoginAsync(
                request.Email,
                request.Password,
                cancellationToken);

            return Ok(response);
        }
    }
}