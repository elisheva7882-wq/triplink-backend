using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using TripLink.Core.DTOs.Rides;
using TripLink.Core.Entities;
using TripLink.Core.Exceptions;
using TripLink.Core.Interfaces.Persistence;
using TripLink.Core.Interfaces.Repositories;
using TripLink.Service.Services;

namespace TripLink.Tests.Services
{
    public class RideServiceTests
    {
        [Fact]
        public async Task GetRideAsync_WhenRideExists_ShouldReturnRide()
        {
            // Arrange
            var rideRepository = new Mock<IRideRepository>();
            var userRepository = new Mock<IUserRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var mapper = new Mock<IMapper>();
            var logger = new Mock<ILogger<RideService>>();

            var ride = new Ride
            {
                Id = 1,
                OwnerId = 10,
                OriginCity = "Jerusalem",
                DestinationCity = "Tel Aviv",
                DepartureAtUtc = DateTime.UtcNow.AddHours(2),
                Capacity = 3,
                BookedSeats = 1
            };

            var rideResponse = new RideResponse
            {
                Id = 1,
                OriginCity = "Jerusalem",
                DestinationCity = "Tel Aviv",
                DepartureAtUtc = ride.DepartureAtUtc,
                Capacity = 3,
                BookedSeats = 1
            };

            rideRepository
                .Setup(r => r.GetByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ride);

            mapper
                .Setup(m => m.Map<RideResponse>(ride))
                .Returns(rideResponse);

            var service = new RideService(
                rideRepository.Object,
                userRepository.Object,
                unitOfWork.Object,
                mapper.Object,
                logger.Object);

            // Act
            var result = await service.GetRideAsync(
                1,
                CancellationToken.None);

            // Assert
            Assert.Equal(1, result.Id);
            Assert.Equal("Jerusalem", result.OriginCity);
            Assert.Equal("Tel Aviv", result.DestinationCity);
            Assert.Equal(3, result.Capacity);
            Assert.Equal(1, result.BookedSeats);
        }

        [Fact]
        public async Task GetRideAsync_WhenRideDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            var rideRepository = new Mock<IRideRepository>();
            var userRepository = new Mock<IUserRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var mapper = new Mock<IMapper>();
            var logger = new Mock<ILogger<RideService>>();

            rideRepository
                .Setup(r => r.GetByIdAsync(
                    999,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Ride?)null);

            var service = new RideService(
                rideRepository.Object,
                userRepository.Object,
                unitOfWork.Object,
                mapper.Object,
                logger.Object);

            // Act
            var exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => service.GetRideAsync(
                        999,
                        CancellationToken.None));

            // Assert
            Assert.Equal(
                "Ride not found.",
                exception.Message);
        }
    }
}