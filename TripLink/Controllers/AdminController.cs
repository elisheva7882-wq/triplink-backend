using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripLink.Core.DTOs.Rides;
using TripLink.Core.Interfaces.Services;

namespace TripLink.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IRideService _rideService;
        private readonly IAdminService _adminService;

        public AdminController(
            IRideService rideService,
            IAdminService adminService)
        {
            _rideService = rideService;
            _adminService = adminService;
        }

        [HttpGet("rides")]
        public async Task<IActionResult> GetRides(
            [FromQuery] RideSearchFilter filter,
            CancellationToken cancellationToken)
        {
            var result = await _rideService.SearchRidesAsync(
                filter,
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            CancellationToken cancellationToken)
        {
            var result = await _adminService.GetUsersAsync(
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("users/{userId:int}")]
        public async Task<IActionResult> GetUser(
            int userId,
            CancellationToken cancellationToken)
        {
            var result = await _adminService.GetUserAsync(
                userId,
                cancellationToken);

            return Ok(result);
        }
        [HttpDelete("rides/{rideId:int}")]
        public async Task<IActionResult> DeleteRide(
    int rideId,
    CancellationToken cancellationToken)
        {
            await _adminService.DeleteRideAsync(
                rideId,
                cancellationToken);

            return NoContent();
        }
    }
}