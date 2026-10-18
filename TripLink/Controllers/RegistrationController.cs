using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TripLink.Core.Interfaces.Services;
using TripLink.Core.DTOs.Registrations;

namespace TripLink.API.Controllers
{
    [ApiController]
    [Route("api/rides")]
    [Authorize]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;

        public RegistrationController(
            IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        [HttpPost("{rideId:int}/register")]
        public async Task<IActionResult> Register(
            int rideId,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int passengerId))
            {
                return Unauthorized();
            }

            var registration =
                await _registrationService.RegisterAsync(
                    rideId,
                    passengerId,
                    cancellationToken);

            return StatusCode(201, new
            {
                registration.RideId,
                registration.PassengerId,
                registration.RegisteredAtUtc
            });
        }

        [HttpDelete("{rideId:int}/register")]
        public async Task<IActionResult> Cancel(
            int rideId,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int passengerId))
            {
                return Unauthorized();
            }

            await _registrationService.CancelAsync(
                rideId,
                passengerId,
                cancellationToken);

            return NoContent();
        }

        [HttpGet("/api/registrations/me")]
        public async Task<IActionResult> GetMine(
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int passengerId))
            {
                return Unauthorized();
            }

            var registrations =
                await _registrationService.GetByPassengerAsync(
                    passengerId,
                    cancellationToken);

            var result = registrations.Select(
                registration => new RegistrationResponse
                {
                    RideId = registration.RideId,
                    PassengerId = registration.PassengerId,
                    RegisteredAtUtc = registration.RegisteredAtUtc
                });

            return Ok(result);
        }

        [HttpGet("{rideId:int}/registrations")]
        public async Task<ActionResult<List<RidePassengerResponse>>> GetRidePassengers(
    int rideId,
    CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var passengers = await _registrationService.GetRidePassengersAsync(
                rideId,
                userId,
                cancellationToken);

            return Ok(passengers);
        }

        private bool TryGetUserId(out int userId)
        {
            var value = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return int.TryParse(value, out userId);
        }

        [HttpDelete("{rideId:int}/registrations/{passengerId:int}")]
        public async Task<IActionResult> RemovePassenger(
    int rideId,
    int passengerId,
    CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            await _registrationService.RemovePassengerAsync(
                rideId,
                passengerId,
                userId,
                cancellationToken);

            return NoContent();
        }
    }
}