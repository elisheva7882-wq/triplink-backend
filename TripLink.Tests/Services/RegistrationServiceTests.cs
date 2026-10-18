using Microsoft.Extensions.Logging;
using Moq;
using TripLink.Core.Entities;
using TripLink.Core.Exceptions;
using TripLink.Core.Interfaces.Persistence;
using TripLink.Core.Interfaces.Repositories;
using TripLink.Service.Services;
using Xunit;

namespace TripLink.Tests.Services
{
    public class RegistrationServiceTests
    {
        [Fact]
        public async Task RegisterAsync_WhenRideIsFull_ShouldThrowDomainConflictException()
        {
            // Arrange
            var rideRepository = new Mock<IRideRepository>();
            var registrationRepository =
                new Mock<IRideRegistrationRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var transaction = new Mock<IAppTransaction>();
            var logger = new Mock<ILogger<RegistrationService>>();

            unitOfWork
                .Setup(u => u.BeginTransactionAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction.Object);

            transaction
                .Setup(t => t.RollbackAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var ride = new Ride
            {
                Id = 1,
                OwnerId = 1,
                Capacity = 1,
                BookedSeats = 1,
                DepartureAtUtc = DateTime.UtcNow.AddHours(2)
            };

            rideRepository
                .Setup(r => r.GetByIdForUpdateAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ride);

            registrationRepository
                .Setup(r => r.ExistsAsync(
                    1,
                    2,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var service = new RegistrationService(
     rideRepository.Object,
     registrationRepository.Object,
     unitOfWork.Object,
     logger.Object);

            // Act
            var exception =
                await Assert.ThrowsAsync<DomainConflictException>(
                    () => service.RegisterAsync(
                        1,
                        2,
                        CancellationToken.None));

            // Assert
            Assert.Equal(
                "No available seats.",
                exception.Message);

            unitOfWork.Verify(
                u => u.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
        [Fact]
        public async Task RegisterAsync_WhenSeatIsAvailable_ShouldRegisterPassenger()
        {
            // Arrange
            var rideRepository = new Mock<IRideRepository>();
            var registrationRepository =
                new Mock<IRideRegistrationRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var transaction = new Mock<IAppTransaction>();
            var logger = new Mock<ILogger<RegistrationService>>();

            unitOfWork
                .Setup(u => u.BeginTransactionAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction.Object);

            unitOfWork
                .Setup(u => u.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            transaction
                .Setup(t => t.CommitAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var ride = new Ride
            {
                Id = 1,
                OwnerId = 1,
                Capacity = 1,
                BookedSeats = 0,
                DepartureAtUtc = DateTime.UtcNow.AddHours(2)
            };

            rideRepository
                .Setup(r => r.GetByIdForUpdateAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ride);

            registrationRepository
                .Setup(r => r.ExistsAsync(
                    1,
                    2,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var service = new RegistrationService(
    rideRepository.Object,
    registrationRepository.Object,
    unitOfWork.Object,
    logger.Object);

            // Act
            var result = await service.RegisterAsync(
                1,
                2,
                CancellationToken.None);

            // Assert
            Assert.Equal(1, ride.BookedSeats);

            Assert.Equal(1, result.RideId);
            Assert.Equal(2, result.PassengerId);

            registrationRepository.Verify(
                r => r.AddAsync(
                    It.Is<RideRegistration>(
                        registration =>
                            registration.RideId == 1 &&
                            registration.PassengerId == 2),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWork.Verify(
                u => u.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            transaction.Verify(
                t => t.CommitAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        [Fact]
        public async Task CancelAsync_WhenRegistrationExists_ShouldCancelAndFreeSeat()
        {
            // Arrange
            var rideRepository = new Mock<IRideRepository>();
            var registrationRepository =
                new Mock<IRideRegistrationRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var transaction = new Mock<IAppTransaction>();
            var logger = new Mock<ILogger<RegistrationService>>();

            unitOfWork
                .Setup(u => u.BeginTransactionAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction.Object);

            unitOfWork
                .Setup(u => u.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            transaction
                .Setup(t => t.CommitAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var ride = new Ride
            {
                Id = 1,
                OwnerId = 1,
                Capacity = 3,
                BookedSeats = 1,
                DepartureAtUtc = DateTime.UtcNow.AddHours(2)
            };

            var registration = new RideRegistration
            {
                RideId = 1,
                PassengerId = 2,
                RegisteredAtUtc = DateTime.UtcNow
            };

            rideRepository
                .Setup(r => r.GetByIdForUpdateAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ride);

            registrationRepository
                .Setup(r => r.GetAsync(
                    1,
                    2,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(registration);

            var service = new RegistrationService(
    rideRepository.Object,
    registrationRepository.Object,
    unitOfWork.Object,
    logger.Object);

            // Act
            await service.CancelAsync(
                1,
                2,
                CancellationToken.None);

            // Assert
            Assert.Equal(0, ride.BookedSeats);

            registrationRepository.Verify(
                r => r.Remove(registration),
                Times.Once);

            unitOfWork.Verify(
                u => u.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            transaction.Verify(
                t => t.CommitAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        [Fact]
        public async Task RegisterAsync_WhenPassengerIsRideOwner_ShouldThrowDomainConflictException()
        {
            // Arrange
            var rideRepository = new Mock<IRideRepository>();
            var registrationRepository =
                new Mock<IRideRegistrationRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var transaction = new Mock<IAppTransaction>();
            var logger = new Mock<ILogger<RegistrationService>>();

            unitOfWork
                .Setup(u => u.BeginTransactionAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction.Object);

            transaction
                .Setup(t => t.RollbackAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var ride = new Ride
            {
                Id = 1,
                OwnerId = 2,
                Capacity = 3,
                BookedSeats = 0,
                DepartureAtUtc = DateTime.UtcNow.AddHours(2)
            };

            rideRepository
                .Setup(r => r.GetByIdForUpdateAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ride);

            var service = new RegistrationService(
    rideRepository.Object,
    registrationRepository.Object,
    unitOfWork.Object,
    logger.Object);

            // Act
            var exception =
                await Assert.ThrowsAsync<DomainConflictException>(
                    () => service.RegisterAsync(
                        1,
                        2,
                        CancellationToken.None));

            // Assert
            Assert.Equal(
                "You cannot register for your own ride.",
                exception.Message);

            registrationRepository.Verify(
                r => r.AddAsync(
                    It.IsAny<RideRegistration>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            unitOfWork.Verify(
                u => u.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
        [Fact]
        public async Task RegisterAsync_WhenAlreadyRegistered_ShouldThrowDomainConflictException()
        {
            // Arrange
            var rideRepository = new Mock<IRideRepository>();
            var registrationRepository =
                new Mock<IRideRegistrationRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var transaction = new Mock<IAppTransaction>();
            var logger = new Mock<ILogger<RegistrationService>>();

            unitOfWork
                .Setup(u => u.BeginTransactionAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction.Object);

            transaction
                .Setup(t => t.RollbackAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var ride = new Ride
            {
                Id = 1,
                OwnerId = 1,
                Capacity = 3,
                BookedSeats = 1,
                DepartureAtUtc = DateTime.UtcNow.AddHours(2)
            };

            rideRepository
                .Setup(r => r.GetByIdForUpdateAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ride);

            registrationRepository
                .Setup(r => r.ExistsAsync(
                    1,
                    2,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = new RegistrationService(
    rideRepository.Object,
    registrationRepository.Object,
    unitOfWork.Object,
    logger.Object);

            // Act
            var exception =
                await Assert.ThrowsAsync<DomainConflictException>(
                    () => service.RegisterAsync(
                        1,
                        2,
                        CancellationToken.None));

            // Assert
            Assert.Equal(
                "You are already registered for this ride.",
                exception.Message);

            Assert.Equal(1, ride.BookedSeats);

            registrationRepository.Verify(
                r => r.AddAsync(
                    It.IsAny<RideRegistration>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            unitOfWork.Verify(
                u => u.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);

            transaction.Verify(
                t => t.RollbackAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        [Fact]
        public async Task RegisterAsync_WhenRideDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            var rideRepository = new Mock<IRideRepository>();
            var registrationRepository =
                new Mock<IRideRegistrationRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var transaction = new Mock<IAppTransaction>();
            var logger = new Mock<ILogger<RegistrationService>>();

            unitOfWork
                .Setup(u => u.BeginTransactionAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction.Object);

            transaction
                .Setup(t => t.RollbackAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            rideRepository
                .Setup(r => r.GetByIdForUpdateAsync(
                    999,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Ride?)null);

            var service = new RegistrationService(
    rideRepository.Object,
    registrationRepository.Object,
    unitOfWork.Object,
    logger.Object);

            // Act
            var exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () => service.RegisterAsync(
                        999,
                        2,
                        CancellationToken.None));

            // Assert
            Assert.Equal("Ride not found.", exception.Message);

            unitOfWork.Verify(
                u => u.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
        [Fact]
        public async Task RegisterAsync_WhenRideAlreadyDeparted_ShouldThrowDomainConflictException()
        {
            // Arrange
            var rideRepository = new Mock<IRideRepository>();
            var registrationRepository =
                new Mock<IRideRegistrationRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var transaction = new Mock<IAppTransaction>();
            var logger = new Mock<ILogger<RegistrationService>>();

            unitOfWork
                .Setup(u => u.BeginTransactionAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction.Object);

            transaction
                .Setup(t => t.RollbackAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var ride = new Ride
            {
                Id = 1,
                OwnerId = 1,
                Capacity = 3,
                BookedSeats = 0,

                // הנסיעה כבר יצאה
                DepartureAtUtc = DateTime.UtcNow.AddHours(-1)
            };

            rideRepository
                .Setup(r => r.GetByIdForUpdateAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ride);

            var service = new RegistrationService(
    rideRepository.Object,
    registrationRepository.Object,
    unitOfWork.Object,
    logger.Object);

            // Act
            var exception =
                await Assert.ThrowsAsync<DomainConflictException>(
                    () => service.RegisterAsync(
                        1,
                        2,
                        CancellationToken.None));

            // Assert
            Assert.Equal(
                "The ride has already departed.",
                exception.Message);

            registrationRepository.Verify(
                r => r.AddAsync(
                    It.IsAny<RideRegistration>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            unitOfWork.Verify(
                u => u.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}