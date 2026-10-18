using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TripLink.Core.DTOs.Rides;
using TripLink.Core.Interfaces.Services;

namespace TripLink.API.Controllers
{
    [ApiController]
    [Route("api/rides")]
    [Authorize]
    public class RideController : ControllerBase
    {
        private readonly IRideService _rideService;

        public RideController(IRideService rideService)
        {
            _rideService = rideService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRide(
            CreateRideRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int ownerId))
            {
                return Unauthorized();
            }

            var result = await _rideService.CreateRideAsync(
                request,
                ownerId,
                cancellationToken);

            return CreatedAtAction(
    nameof(GetRide),
    new { rideId = result.Id },
    result);
        }

        [HttpGet("{rideId:int}")]
        public async Task<IActionResult> GetRide(
            int rideId,
            CancellationToken cancellationToken)
        {
            var result = await _rideService.GetRideAsync(
                rideId,
                cancellationToken);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> SearchRides(
            [FromQuery] RideSearchFilter filter,
            CancellationToken cancellationToken)
        {
            var result = await _rideService.SearchRidesAsync(
                filter,
                cancellationToken);

            return Ok(result);
        }

        [HttpPut("{rideId:int}")]
        public async Task<IActionResult> UpdateRide(
            int rideId,
            UpdateRideRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int userId))
            {
                return Unauthorized();
            }

            await _rideService.UpdateRideAsync(
                rideId,
                userId,
                request,
                cancellationToken);

            return NoContent();
        }

        [HttpGet("mine")]
        public async Task<ActionResult<List<RideResponse>>> GetMyRides(
    CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var rides = await _rideService.GetMyRidesAsync(
                userId,
                cancellationToken);

            return Ok(rides);
        }

        [HttpDelete("{rideId:int}")]
        public async Task<IActionResult> DeleteRide(
            int rideId,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int userId))
            {
                return Unauthorized();
            }

            await _rideService.DeleteRideAsync(
                rideId,
                userId,
                cancellationToken);

            return NoContent();
        }

        private bool TryGetUserId(out int userId)
        {
            var value = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return int.TryParse(value, out userId);
        }
    }
}