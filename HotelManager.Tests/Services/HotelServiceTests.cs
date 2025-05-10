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
using HotelManager.Data.Sorting;

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
            var hotel = new Hotel
            {
                Id = Guid.NewGuid(),
                Name = "Test Hotel",
                City = "Test City",
                RoomsPerFloor = 10,
                Rooms = new List<Room>()
            };
            _hotelRepositoryMock.Setup(x => x.Create(It.IsAny<Hotel>()));

            var result = _sut.Create(hotel);

            result.Should().BeTrue();
            _hotelRepositoryMock.Verify(x => x.Create(It.Is<Hotel>(h => 
                h.Id == hotel.Id && 
                h.Name == hotel.Name)), 
                Times.Once);
        }

        [Fact]
        public void GetRoomsPerFloor_ExistingHotel_ReturnsCorrectNumber()
        {
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

            var result = _sut.GetRoomsPerFloor(hotelId);

            result.Should().Be(expectedRoomsPerFloor);
        }

        [Fact]
        public void GetHotelInfo_ValidId_ReturnsCorrectData()
        {
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

            var result = _sut.GetHotelInfo(hotelId);

            result.Should().NotBeNull();
            result.HotelName.Should().Be(hotel.Name);
        }

        [Fact]
        public void GetHotelInfo_ShouldReturnDashboardData()
        {
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

            var result = _sut.GetHotelInfo(hotelId);

            result.Should().NotBeNull();
            result.HotelName.Should().Be(hotel.Name);
        }

        [Fact]
        public void GetMonthlyRevenue_ShouldCalculateCorrectly()
        {
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

            var result = _sut.GetMonthlyRevenue(hotelId);


            result.Should().Be(200); 
        }

        [Fact]
        public void GetCurrentGuestsCount_ShouldReturnCorrectCount()
        {
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

            var result = _sut.GetCurrentGuestsCount(hotelId);

            result.Should().Be(0);
        }
[Fact]
        public void GetAllBookings_ShouldReturnBookingsForGivenHotelId_OrderedByCheckInDescending()
        {
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


            var result = _sut.GetAllBookings(hotelId).ToList();


            result.Should().HaveCount(2);
            result[0].CheckIn.Should().BeAfter(result[1].CheckIn); 
            result.All(b => b.Room.HotelId == hotelId).Should().BeTrue();
        }
        [Fact]
        public void GetAllRooms_ShouldReturnRoomsForGivenHotelId()
        {

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


            var result = _sut.GetAllRooms(hotelId).ToList();


            result.Should().HaveCount(2);
            result.All(r => r.HotelId == hotelId).Should().BeTrue();
        }
        [Fact]
        public void GetAllMinified_ShouldReturnAllHotelsMinified()
        {
            var hotels = new List<Hotel>
            {
                new Hotel { Id = Guid.NewGuid(), Name = "Hotel Alpha" },
                new Hotel { Id = Guid.NewGuid(), Name = "Hotel Beta" }
            };

            _hotelRepositoryMock
                .Setup(r => r.GetMany(
                    It.IsAny<Expression<Func<Hotel, bool>>>(),
                    It.IsAny<Expression<Func<Hotel, HotelMinifiedInfoProjection>>>(),
                    It.IsAny<IEnumerable<IOrderClause<Hotel>>>()))
                .Returns((Expression<Func<Hotel, bool>> filterExpr,
                          Expression<Func<Hotel, HotelMinifiedInfoProjection>> selectorExpr,
                          IEnumerable<IOrderClause<Hotel>> order) =>
                {
                    var filter = filterExpr.Compile();
                    var selector = selectorExpr.Compile();
                    return hotels.Where(filter).Select(selector);
                });

            var result = _sut.GetAllMinified().ToList();

            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Hotel Alpha");
            result[1].Name.Should().Be("Hotel Beta");
        }
        [Fact]
        public void GetAll_ShouldReturnAllHotelsWithRooms()
        {
            var hotelId = Guid.NewGuid();
            var hotels = new List<Hotel>
            {
                new Hotel
                {
                    Id = hotelId,
                    Name = "Hotel Alpha",
                    Address = "123 Main St",
                    City = "Metropolis",
                    Email = "alpha@hotel.com",
                    RoomsPerFloor = 10,
                    Rooms = new List<Room>
                    {
                        new Room
                        {
                            Id = Guid.NewGuid(),
                            Number = 101,
                            PricePerNight = 100,
                            HotelId = hotelId,
                            Status = "Available",
                            Type = "Standard"
                        },
                        new Room
                        {
                            Id = Guid.NewGuid(),
                            Number = 102,
                            PricePerNight = 120,
                            HotelId = hotelId,
                            Status = "Occupied",
                            Type = "Suite"
                        }
                    }
                }
            };

            _hotelRepositoryMock
                .Setup(r => r.GetMany(
                    It.IsAny<Expression<Func<Hotel, bool>>>(),
                    It.IsAny<Expression<Func<Hotel, HotelGeneralInfoProjection>>>(),
                    It.IsAny<IEnumerable<IOrderClause<Hotel>>>()))
                .Returns((Expression<Func<Hotel, bool>> filterExpr,
                          Expression<Func<Hotel, HotelGeneralInfoProjection>> selectorExpr,
                          IEnumerable<IOrderClause<Hotel>> order) =>
                {
                    var filter = filterExpr.Compile();
                    var selector = selectorExpr.Compile();
                    return hotels.Where(filter).Select(selector);
                });

            var result = _sut.GetAll().ToList();

            result.Should().HaveCount(1);
            var hotel = result.First();
            hotel.Name.Should().Be("Hotel Alpha");
            hotel.Address.Should().Be("123 Main St");
            hotel.City.Should().Be("Metropolis");
            hotel.Email.Should().Be("alpha@hotel.com");
            hotel.RoomsPerFloor.Should().Be(10);
            hotel.Rooms.Should().HaveCount(2);
            hotel.Rooms.ElementAt(0).Number.Should().Be(101);
            hotel.Rooms.ElementAt(1).Number.Should().Be(102);
        }
    }
}