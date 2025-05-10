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

        /// <summary>
        /// Tests the <see cref="HotelService.Create"/> method to ensure it successfully creates a valid hotel.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method calls the repository's <see cref="IRepository{T}.Create"/> method with the correct hotel object.
        /// It mocks the repository to simulate the creation process and checks that the method returns <c>true</c> upon successful creation.
        /// The test also ensures that the repository's <see cref="IRepository{T}.Create"/> method is called exactly once with the expected hotel object.
        /// </remarks>
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

        /// <summary>
        /// Tests the <see cref="HotelService.GetRoomsPerFloor"/> method to ensure it returns the correct number of rooms per floor for a given hotel ID.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method retrieves the correct number of rooms per floor for the specified hotel ID.
        /// It mocks the <see cref="IRepository{Hotel}.Get"/> method to return a predefined hotel object with the expected number of rooms per floor.
        /// The test ensures that the result matches the expected value.
        /// </remarks>
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

        /// <summary>
        /// Tests the <see cref="HotelService.GetHotelInfo"/> method to ensure it returns the correct data for a valid hotel ID.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method retrieves the hotel information for the given hotel ID.
        /// It mocks the <see cref="IRepository{Hotel}.Get"/> method to return a predefined hotel object.
        /// The test ensures that the result is not null and contains the correct hotel name.
        /// </remarks>
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

        /// <summary>
        /// Tests the <see cref="HotelService.GetHotelInfo"/> method to ensure it returns the correct dashboard data for the specified hotel ID.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method retrieves the hotel information, bookings, and rooms for the given hotel ID.
        /// It mocks the <see cref="IRepository{Hotel}.Get"/> method to return a predefined hotel object, 
        /// and the <see cref="IBookingService.GetAll"/> and <see cref="IRoomService.GetAll"/> methods to return empty lists.
        /// The test ensures that the result contains the correct hotel name and is not null.
        /// </remarks>
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

        /// <summary>
        /// Tests the <see cref="HotelService.GetMonthlyRevenue"/> method to ensure it calculates the correct revenue for the specified hotel ID.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method sums up the revenue generated by bookings for the given hotel ID within the current month.
        /// It mocks the <see cref="IBookingService.GetAll"/> method to return a predefined list of bookings and ensures the result matches the expected revenue.
        /// </remarks>
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
                        Type = "Single",
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

        /// <summary>
        /// Tests the <see cref="HotelService.GetCurrentGuestsCount"/> method to ensure it returns the correct count of current guests for the specified hotel ID.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method filters bookings by the provided hotel ID and checks if the current date falls within the booking's check-in and check-out dates.
        /// It mocks the <see cref="IBookingService.GetAll"/> method to return a predefined list of bookings and ensures the result matches the expected count.
        /// </remarks>
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

        /// <summary>
        /// Tests the <see cref="HotelService.GetAllBookings"/> method to ensure it returns only the bookings associated with the specified hotel ID, ordered by check-in date in descending order.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method filters bookings by the provided hotel ID and correctly orders them by check-in date in descending order.
        /// It mocks the <see cref="IBookingService.GetAll"/> method to return a predefined list of bookings and checks that the result contains only bookings belonging to the specified hotel.
        /// </remarks>
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
                        Type = "Single"
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

        /// <summary>
        /// Tests the <see cref="HotelService.GetAllRooms"/> method to ensure it returns only the rooms associated with the specified hotel ID.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method filters rooms by the provided hotel ID and correctly maps them to the <see cref="RoomGeneralInfoProjection"/> format.
        /// It mocks the <see cref="IRoomService.GetAll"/> method to return a predefined list of rooms and checks that the result contains only rooms belonging to the specified hotel.
        /// </remarks>
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
                    Type = "Single",
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

        /// <summary>
        /// Tests the <see cref="HotelService.GetAllMinified"/> method to ensure it returns all hotels in a minified format.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method retrieves all hotels from the repository and correctly maps them to the <see cref="HotelMinifiedInfoProjection"/> format.
        /// It mocks the repository to return a predefined list of hotels and checks that the result matches the expected output.
        /// </remarks>
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

        /// <summary>
        /// Tests the <see cref="HotelService.GetAll"/> method to ensure it returns all hotels with their associated rooms.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method retrieves all hotels from the repository and correctly maps them to the <see cref="HotelGeneralInfoProjection"/> format.
        /// It mocks the repository to return a predefined list of hotels and checks that the result matches the expected output, including the associated rooms.
        /// </remarks>
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
                            Type = "Single"
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