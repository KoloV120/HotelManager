using Xunit;
using Moq;
using FluentAssertions;
using HotelManager.Core.Services;
using HotelManager.Data.Models;
using HotelManager.Data.Repositories;
using HotelManager.Core.Projections.Hotels;
using System.Linq.Expressions;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Projections.Bookings;
using HotelManager.Core.Projections.Rooms;
using HotelManager.Core.Projections.Guests;

namespace HotelManager.Tests.Services
{
    public class HotelServiceTests
    {
        private readonly Mock<IRepository<Hotel>> _hotelRepositoryMock;
        private readonly Mock<IBookingService> _bookingServiceMock;
        private readonly Mock<IRoomService> _roomServiceMock;
        private readonly HotelService _sut;

        public HotelServiceTests()
        {
            _hotelRepositoryMock = new Mock<IRepository<Hotel>>();
            _bookingServiceMock = new Mock<IBookingService>();
            _roomServiceMock = new Mock<IRoomService>();
            _sut = new HotelService(_hotelRepositoryMock.Object, _bookingServiceMock.Object, _roomServiceMock.Object);
        }

        [Fact]
        public void Create_ValidHotel_ReturnsTrue()
        {
            // Arrange
            var hotel = new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Test Hotel",
                City = "Test City",
                RoomsPerFloor = 10,
                Rooms = new List<Room>()
            };
            _hotelRepositoryMock.Setup(x => x.Create(It.IsAny<Hotel>()));

            // Act
            var result = _sut.Create(hotel);

            // Assert
            result.Should().BeTrue();
            _hotelRepositoryMock.Verify(x => x.Create(It.Is<Hotel>(h => 
                h.Id == hotel.Id && 
                h.Name == hotel.Name)), 
                Times.Once);
        }

        [Fact]
        public void GetRoomsPerFloor_ExistingHotel_ReturnsCorrectNumber()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var expectedRoomsPerFloor = 10;
            var hotel = new Hotel 
            { 
                Id = hotelId, 
                RoomsPerFloor = expectedRoomsPerFloor,
                Rooms = new List<Room>()
            };

            _hotelRepositoryMock.Setup(x => x.Get(It.IsAny<System.Linq.Expressions.Expression<Func<Hotel, bool>>>()))
                .Returns(hotel);

            // Act
            var result = _sut.GetRoomsPerFloor(hotelId);

            // Assert
            result.Should().Be(expectedRoomsPerFloor);
        }

        [Fact]
        public void GetHotelInfo_ValidId_ReturnsCorrectData()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var hotel = new Hotel 
            { 
                Id = hotelId, 
                Name = "Test Hotel",
                RoomsPerFloor = 10,
                Rooms = new List<Room>()
            };

            _hotelRepositoryMock.Setup(x => x.Get(It.IsAny<System.Linq.Expressions.Expression<Func<Hotel, bool>>>()))
                .Returns(hotel);

            // Act
            var result = _sut.GetHotelInfo(hotelId);

            // Assert
            result.Should().NotBeNull();
            result.HotelName.Should().Be(hotel.Name);
        }

        [Fact]
        public void GetHotelInfo_ShouldReturnDashboardData()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var hotel = new Hotel 
            { 
                Id = hotelId, 
                Name = "Test Hotel",
                RoomsPerFloor = 10
            };

            _hotelRepositoryMock
                .Setup(x => x.Get(It.IsAny<Expression<Func<Hotel, bool>>>()))
                .Returns(hotel);

            _bookingServiceMock
                .Setup(x => x.GetAll())
                .Returns(new List<BookingGeneralInfoProjection>());

            _roomServiceMock
                .Setup(x => x.GetAll())
                .Returns(new List<RoomGeneralInfoProjection>());

            // Act
            var result = _sut.GetHotelInfo(hotelId);

            // Assert
            result.Should().NotBeNull();
            result.HotelName.Should().Be(hotel.Name);
        }

        [Fact]
        public void GetMonthlyRevenue_ShouldCalculateCorrectly()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var guestId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            
            var bookings = new List<BookingGeneralInfoProjection>
            {
                new BookingGeneralInfoProjection
                {
                    Id = Guid.NewGuid(),
                    CheckIn = DateTime.Now,
                    CheckOut = DateTime.Now.AddDays(2),
                    Status = "confirmed",
                    Guest = new GuestMinifiedInfoProjection
                    {
                        Id = guestId,
                        Name = "John Doe"
                    },
                    Room = new RoomMinifiedInfoProjection
                    {
                        Id = roomId,
                        Number = 101,
                        Type = "Standard",
                        Status = "Available",
                        PricePerNight = 100,
                        HotelId = hotelId
                    }
                }
            };

            _bookingServiceMock
                .Setup(x => x.GetAll())
                .Returns(bookings);

            // Act
            var result = _sut.GetMonthlyRevenue(hotelId);

            // Assert
            result.Should().Be(200); // 2 nights * 100 per night
        }

        [Fact]
        public void GetCurrentGuestsCount_ShouldReturnCorrectCount()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var guestId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            
            var bookings = new List<BookingGeneralInfoProjection>
            {
                new BookingGeneralInfoProjection
                {
                    Id = Guid.NewGuid(),
                    CheckIn = DateTime.Now.AddDays(-1),
                    CheckOut = DateTime.Now.AddDays(1),
                    Status = "confirmed",
                    Guest = new GuestMinifiedInfoProjection
                    {
                        Id = guestId,
                        Name = "John Doe"
                    },
                    Room = new RoomMinifiedInfoProjection 
                    { 
                        Id = roomId,
                        Number = 101,
                        Type = "Standard",
                        Status = "Available",
                        PricePerNight = 100,
                        HotelId = hotelId 
                    }
                }
            };

            _bookingServiceMock
                .Setup(x => x.GetAll())
                .Returns(bookings);

            // Act
            var result = _sut.GetCurrentGuestsCount(hotelId);

            // Assert
            result.Should().Be(0);
        }
[Fact]
        public void GetAllBookings_ShouldReturnBookingsForGivenHotelId_OrderedByCheckInDescending()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var otherHotelId = Guid.NewGuid();

            var bookings = new List<BookingGeneralInfoProjection>
            {
                new BookingGeneralInfoProjection
                {
                    Id = Guid.NewGuid(),
                    CheckIn = new DateTime(2024, 5, 10),
                    CheckOut = new DateTime(2024, 5, 12),
                    Status = "Confirmed",
                    Guest = new GuestMinifiedInfoProjection { Id = Guid.NewGuid(), Name = "Alice" },
                    Room = new RoomMinifiedInfoProjection
                    {
                        Id = Guid.NewGuid(),
                        Number = 101,
                        PricePerNight = 100,
                        HotelId = hotelId,
                        Status = "Available",
                        Type = "Standard"
                    }
                },
                new BookingGeneralInfoProjection
                {
                    Id = Guid.NewGuid(),
                    CheckIn = new DateTime(2024, 5, 12),
                    CheckOut = new DateTime(2024, 5, 14),
                    Status = "Confirmed",
                    Guest = new GuestMinifiedInfoProjection { Id = Guid.NewGuid(), Name = "Bob" },
                    Room = new RoomMinifiedInfoProjection
                    {
                        Id = Guid.NewGuid(),
                        Number = 102,
                        PricePerNight = 120,
                        HotelId = hotelId,
                        Status = "Available",
                        Type = "Double"
                    }
                },
                // Booking for another hotel (should not be included)
                new BookingGeneralInfoProjection
                {
                    Id = Guid.NewGuid(),
                    CheckIn = new DateTime(2024, 5, 15),
                    CheckOut = new DateTime(2024, 5, 16),
                    Status = "Confirmed",
                    Guest = new GuestMinifiedInfoProjection { Id = Guid.NewGuid(), Name = "Charlie" },
                    Room = new RoomMinifiedInfoProjection
                    {
                        Id = Guid.NewGuid(),
                        Number = 201,
                        PricePerNight = 150,
                        HotelId = otherHotelId,
                        Status = "Available",
                        Type = "Suite"
                    }
                }
            };

            _bookingServiceMock.Setup(x => x.GetAll()).Returns(bookings);

            // Act
            var result = _sut.GetAllBookings(hotelId).ToList();

            // Assert
            result.Should().HaveCount(2);
            result[0].CheckIn.Should().BeAfter(result[1].CheckIn); // Ordered descending
            result.All(b => b.Room.HotelId == hotelId).Should().BeTrue();
        }
        [Fact]
        public void GetAllRooms_ShouldReturnRoomsForGivenHotelId()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var otherHotelId = Guid.NewGuid();

            var rooms = new List<RoomGeneralInfoProjection>
            {
                new RoomGeneralInfoProjection
                {
                    Id = Guid.NewGuid(),
                    Number = 101,
                    HotelId = hotelId,
                    PricePerNight = 100,
                    Status = "Available",
                    Type = "Standard",
                    Bookings = new List<BookingMinifiedInfoProjection>()
                },
                new RoomGeneralInfoProjection
                {
                    Id = Guid.NewGuid(),
                    Number = 102,
                    HotelId = hotelId,
                    PricePerNight = 120,
                    Status = "Occupied",
                    Type = "Double",
                    Bookings = new List<BookingMinifiedInfoProjection>()
                },
                new RoomGeneralInfoProjection
                {
                    Id = Guid.NewGuid(),
                    Number = 201,
                    HotelId = otherHotelId,
                    PricePerNight = 150,
                    Status = "Available",
                    Type = "Suite",
                    Bookings = new List<BookingMinifiedInfoProjection>()
                }
            };

            _roomServiceMock.Setup(x => x.GetAll()).Returns(rooms);

            // Act
            var result = _sut.GetAllRooms(hotelId).ToList();

            // Assert
            result.Should().HaveCount(2);
            result.All(r => r.HotelId == hotelId).Should().BeTrue();
        }
    }
}