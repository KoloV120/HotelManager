using Xunit;
using Moq;
using FluentAssertions;
using System.Linq.Expressions;
using HotelManager.Core.Services;
using HotelManager.Data.Models;
using HotelManager.Data.Repositories;
using HotelManager.Core.Projections.Rooms;
using HotelManager.Core.Projections.Bookings;
using HotelManager.Data.Sorting;

namespace HotelManager.Tests.Services
{
    public class RoomServiceTests
    {
        private readonly Mock<IRepository<Room>> _roomRepositoryMock;
        private readonly RoomService _sut;

        public RoomServiceTests()
        {
            _roomRepositoryMock = new Mock<IRepository<Room>>();
            _sut = new RoomService(_roomRepositoryMock.Object);
        }

        [Fact]
        public void Create_ValidRoom_ReturnsTrue()
        {
            // Arrange
            var room = new Room
            {
                Id = Guid.NewGuid(),
                Number = 101,
                Type = "Single",
                PricePerNight = 100,
                Status = "Available"
            };

            _roomRepositoryMock.Setup(x => x.Create(It.IsAny<Room>()));

            // Act
            var result = _sut.Create(room);

            // Assert
            result.Should().BeTrue();
            _roomRepositoryMock.Verify(x => x.Create(room), Times.Once);
        }

        [Fact]
        public void GetAllByHotelId_ReturnsCorrectRooms()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var rooms = new List<Room>
            {
                new Room
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId,
                    Number = 101,
                    Type = "Standard",
                    PricePerNight = 100,
                    Status = "Available",
                    Bookings = new List<Booking>()
                },
                new Room
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId,
                    Number = 102,
                    Type = "Deluxe",
                    PricePerNight = 150,
                    Status = "Available",
                    Bookings = new List<Booking>()
                }
            };

            _roomRepositoryMock
                .Setup(x => x.GetMany<RoomGeneralInfoProjection>(
                    It.IsAny<Expression<Func<Room, bool>>>(),
                    It.IsAny<Expression<Func<Room, RoomGeneralInfoProjection>>>(),
                    It.IsAny<IEnumerable<IOrderClause<Room>>>()))
                .Returns(rooms.Select(r => new RoomGeneralInfoProjection
                {
                    Id = r.Id,
                    Number = r.Number,
                    Type = r.Type,
                    PricePerNight = r.PricePerNight,
                    Status = r.Status,
                    HotelId = r.HotelId,
                    Bookings = new List<BookingMinifiedInfoProjection>()
                }));

            // Act
            var result = _sut.GetAllByHotelId(hotelId);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
            result.All(r => r.HotelId == hotelId).Should().BeTrue();
        }

        [Fact]
        public void GetAll_ShouldReturnAllRooms()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room
                {
                    Id = Guid.NewGuid(),
                    Number = 101,
                    Type = "Standard",
                    PricePerNight = 100,
                    Status = "Available",
                    HotelId = Guid.NewGuid(),
                    Bookings = new List<Booking>()
                }
            };

            _roomRepositoryMock
                .Setup(x => x.GetMany(
                    It.IsAny<Expression<Func<Room, bool>>>(),
                    It.IsAny<Expression<Func<Room, RoomGeneralInfoProjection>>>(),
                    It.IsAny<IEnumerable<IOrderClause<Room>>>()))
                .Returns(rooms.Select(r => new RoomGeneralInfoProjection
                {
                    Id = r.Id,
                    Number = r.Number,
                    Type = r.Type,
                    PricePerNight = r.PricePerNight,
                    Status = r.Status,
                    HotelId = r.HotelId,
                    Bookings = new List<BookingMinifiedInfoProjection>()
                }));

            // Act
            var result = _sut.GetAll();

            // Assert
            result.Should().NotBeEmpty();
            var room = result.First();
            room.Number.Should().Be(101);
            room.Type.Should().Be("Standard");
        }
    }
}