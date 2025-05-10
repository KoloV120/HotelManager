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


        /// <summary>
        /// Tests the <see cref="RoomService.Create"/> method to ensure it successfully creates a valid room.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method calls the repository's <see cref="IRepository{T}.Create"/> method with the correct room object.
        /// It mocks the repository to simulate the creation process and checks that the method returns <c>true</c> upon successful creation.
        /// </remarks>
        [Fact]
        public void Create_ValidRoom_ReturnsTrue()
        {
            var room = new Room
            {
                Id = Guid.NewGuid(),
                Number = 101,
                Type = "Single",
                PricePerNight = 100,
                Status = "Available"
            };

            _roomRepositoryMock.Setup(x => x.Create(It.IsAny<Room>()));

            var result = _sut.Create(room);

            result.Should().BeTrue();
            _roomRepositoryMock.Verify(x => x.Create(room), Times.Once);
        }


        /// <summary>
        /// Tests the <see cref="RoomService.GetAllByHotelId"/> method to ensure it returns only the rooms associated with the specified hotel ID.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method filters rooms by the provided hotel ID and correctly maps them to the <see cref="RoomGeneralInfoProjection"/> format.
        /// It mocks the repository to return a predefined list of rooms and checks that the result contains only rooms belonging to the specified hotel.
        /// </remarks>
        [Fact]
        public void GetAllByHotelId_ReturnsCorrectRooms()
        {
            var hotelId = Guid.NewGuid();
            var rooms = new List<Room>
            {
                new Room
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId,
                    Number = 101,
                    Type = "Single",
                    PricePerNight = 100,
                    Status = "Available",
                    Bookings = new List<Booking>()
                },
                new Room
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId,
                    Number = 102,
                    Type = "Double",
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

            var result = _sut.GetAllByHotelId(hotelId);

            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
            result.All(r => r.HotelId == hotelId).Should().BeTrue();
        }


        /// <summary>
        /// Tests the <see cref="RoomService.GetAll"/> method to ensure it returns all rooms with general information.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method retrieves all rooms from the repository and correctly maps them to the <see cref="RoomGeneralInfoProjection"/> format.
        /// It mocks the repository to return a predefined list of rooms and checks that the result matches the expected output.
        /// </remarks>
        [Fact]
        public void GetAll_ShouldReturnAllRooms()
        {
            var rooms = new List<Room>
            {
                new Room
                {
                    Id = Guid.NewGuid(),
                    Number = 101,
                    Type = "Single",
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

            var result = _sut.GetAll();

            result.Should().NotBeEmpty();
            var room = result.First();
            room.Number.Should().Be(101);
            room.Type.Should().Be("Single");
        }

        /// <summary>
        /// Tests the <see cref="RoomService.GetAllMinified"/> method to ensure it returns all rooms in a minified format.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method correctly filters and projects room data into a simplified format.
        /// It mocks the repository to return a predefined list of rooms and checks that the result matches the expected output.
        /// </remarks>
        [Fact]
        public void GetAllMinified_ShouldReturnAllRoomsMinified()
        {
            var rooms = new List<Room>
            {
                new Room
                {
                    Id = Guid.NewGuid(),
                    Number = 101,
                    PricePerNight = 100,
                    Status = "Available",
                    HotelId = Guid.NewGuid(),
                    Type = "Single"
                },
                new Room
                {
                    Id = Guid.NewGuid(),
                    Number = 102,
                    PricePerNight = 120,
                    Status = "Occupied",
                    HotelId = Guid.NewGuid(),
                    Type = "Suite"
                }
            };

            _roomRepositoryMock
                .Setup(r => r.GetMany(
                    It.IsAny<Expression<Func<Room, bool>>>(),
                    It.IsAny<Expression<Func<Room, RoomMinifiedInfoProjection>>>(),
                    It.IsAny<IEnumerable<IOrderClause<Room>>>()))
                .Returns((Expression<Func<Room, bool>> filterExpr,
                          Expression<Func<Room, RoomMinifiedInfoProjection>> selectorExpr,
                          IEnumerable<IOrderClause<Room>> order) =>
                {
                    var filter = filterExpr.Compile();
                    var selector = selectorExpr.Compile();
                    return rooms.Where(filter).Select(selector);
                });

            var result = _sut.GetAllMinified().ToList();

            result.Should().HaveCount(2);
            result[0].Number.Should().Be(101);
            result[1].Number.Should().Be(102);
        }
    }
}