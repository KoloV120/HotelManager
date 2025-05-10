using Xunit;
using Moq;
using FluentAssertions;
using HotelManager.Core.Services;
using HotelManager.Data.Models;
using HotelManager.Data.Repositories;
using HotelManager.Core.Projections.Bookings;
using System.Linq.Expressions;
using HotelManager.Core.Projections.Guests;
using HotelManager.Core.Projections.Rooms;
using HotelManager.Data.Sorting;

namespace HotelManager.Tests.Services
{
    public class BookingServiceTests
    {
        private readonly Mock<IRepository<Booking>> _bookingRepositoryMock;
        private readonly BookingService _sut;

        public BookingServiceTests()
        {
            _bookingRepositoryMock = new Mock<IRepository<Booking>>();
            _sut = new BookingService(_bookingRepositoryMock.Object);
        }

        /// <summary>
        /// Tests the <see cref="BookingService.IsRoomAvailable"/> method to ensure it returns <c>true</c> when there are no conflicting bookings for the specified room and dates.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method correctly determines room availability when no bookings overlap with the specified check-in and check-out dates.
        /// It mocks the <see cref="IRepository{Booking}.GetMany"/> method to return an empty list of bookings, simulating no conflicts.
        /// The test ensures that the result is <c>true</c>.
        /// </remarks>
        [Fact]
        public void IsRoomAvailable_NoConflictingBookings_ReturnsTrue()
        {
            var roomId = Guid.NewGuid();
            var checkIn = DateTime.Today.AddDays(1);
            var checkOut = DateTime.Today.AddDays(3);

            _bookingRepositoryMock.Setup(x => x.GetMany(
                It.IsAny<System.Linq.Expressions.Expression<Func<Booking, bool>>>()))
                .Returns(new List<Booking>());

            var result = _sut.IsRoomAvailable(roomId, checkIn, checkOut);

            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests the <see cref="BookingService.Create"/> method to ensure it successfully creates a valid booking.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method calls the repository's <see cref="IRepository{T}.Create"/> method with the correct booking object.
        /// It mocks the repository to simulate the creation process and checks that the method returns <c>true</c> upon successful creation.
        /// The test also ensures that the repository's <see cref="IRepository{T}.Create"/> method is called exactly once with the expected booking object.
        /// </remarks>
        [Fact]
        public void Create_ValidBooking_ReturnsTrue()
        {
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                CheckIn = DateTime.Today.AddDays(1),
                CheckOut = DateTime.Today.AddDays(3),
                Status = "confirmed"
            };

            _bookingRepositoryMock.Setup(x => x.Create(It.IsAny<Booking>()));

            var result = _sut.Create(booking);

            result.Should().BeTrue();
            _bookingRepositoryMock.Verify(x => x.Create(booking), Times.Once);
        }

        /// <summary>
        /// Tests the <see cref="BookingService.GetAll"/> method to ensure it returns all bookings with their general information.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method retrieves all bookings and maps them to the <see cref="BookingGeneralInfoProjection"/> format.
        /// It mocks the <see cref="IRepository{Booking}.GetMany"/> method to return a predefined list of bookings.
        /// The test ensures that the result is not empty and that each booking contains valid guest and room information.
        /// </remarks>
        [Fact]
        public void GetAll_ShouldReturnAllBookings()
        {

            var bookings = new List<Booking>
            {
                new Booking 
                { 
                    Id = Guid.NewGuid(),
                    CheckIn = DateTime.Today,
                    CheckOut = DateTime.Today.AddDays(2),
                    Status = "confirmed",
                    Guest = new Guest { Id = Guid.NewGuid(), Name = "John Doe" },
                    Room = new Room { Id = Guid.NewGuid(), Number = 101 }
                }
            };

            _bookingRepositoryMock
                .Setup(x => x.GetMany(
                    It.IsAny<Expression<Func<Booking, bool>>>(),
                    It.IsAny<Expression<Func<Booking, BookingGeneralInfoProjection>>>(),
                    It.IsAny<IEnumerable<IOrderClause<Booking>>>()))
                .Returns(bookings.Select(b => new BookingGeneralInfoProjection
                {
                    Id = b.Id,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    Status = b.Status,
                    Guest = new GuestMinifiedInfoProjection
                    {
                        Id = b.Guest.Id,
                        Name = b.Guest.Name
                    },
                    Room = new RoomMinifiedInfoProjection
                    {
                        Id = b.Room.Id,
                        Number = b.Room.Number,
                        Status = "Available",
                        Type = "Single",
                        PricePerNight = 100,
                        HotelId = Guid.NewGuid()
                    }
                }));

            var result = _sut.GetAll();

            result.Should().NotBeEmpty();
            result.First().Guest.Should().NotBeNull();
            result.First().Room.Should().NotBeNull();
        }

        /// <summary>
        /// Tests the <see cref="BookingService.IsRoomAvailable"/> method to ensure it returns the correct room availability status.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method correctly determines whether a room is available for the specified check-in and check-out dates.
        /// It uses a parameterized test with <see cref="InlineDataAttribute"/> to test both availability scenarios (true and false).
        /// It mocks the <see cref="IRepository{Booking}.GetMany"/> method to return a predefined list of bookings based on the expected availability.
        /// The test ensures that the result matches the expected availability status.
        /// </remarks>
        [Theory]
        [InlineData(true)]  
        [InlineData(false)] 
        public void IsRoomAvailable_ShouldReturnCorrectAvailability(bool expectedAvailability)
        {
            var roomId = Guid.NewGuid();
            var checkIn = DateTime.Today.AddDays(1);
            var checkOut = DateTime.Today.AddDays(3);
            var guestId = Guid.NewGuid();
            
            var bookings = expectedAvailability 
                ? new List<BookingGeneralInfoProjection>()
                : new List<BookingGeneralInfoProjection> 
                {
                    new BookingGeneralInfoProjection 
                    { 
                        Id = Guid.NewGuid(),
                        CheckIn = checkIn,
                        CheckOut = checkOut,
                        Status = "confirmed",
                        Room = new RoomMinifiedInfoProjection 
                        { 
                            Id = roomId,
                            Number = 101,
                            Type = "Single",
                            Status = "Available",
                            PricePerNight = 100,
                            HotelId = Guid.NewGuid()
                        },
                        Guest = new GuestMinifiedInfoProjection 
                        { 
                            Id = guestId,
                            Name = "John Doe"
                        }
                    }
                };

            _bookingRepositoryMock
                .Setup(x => x.GetMany<BookingGeneralInfoProjection>(
                    It.IsAny<Expression<Func<Booking, bool>>>(),
                    It.IsAny<Expression<Func<Booking, BookingGeneralInfoProjection>>>(),
                    It.IsAny<IEnumerable<IOrderClause<Booking>>>()))
                .Returns(bookings);

            var result = _sut.IsRoomAvailable(roomId, checkIn, checkOut);

            result.Should().Be(expectedAvailability);
        }

        /// <summary>
        /// Tests the <see cref="BookingService.GetAllMinified"/> method to ensure it returns all bookings in a minified format.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method retrieves all bookings and maps them to the <see cref="BookingMinifiedInfoProjection"/> format.
        /// It mocks the <see cref="IRepository{Booking}.GetMany"/> method to return a predefined list of bookings.
        /// The test ensures that the result contains the expected number of bookings and verifies the check-in dates of the returned bookings.
        /// </remarks>
        [Fact]
        public void GetAllMinified_ShouldReturnAllBookingsMinified()
        {
            var bookings = new List<Booking>
            {
                new Booking
                {
                    Id = Guid.NewGuid(),
                    CheckIn = new DateTime(2024, 5, 10),
                    CheckOut = new DateTime(2024, 5, 12)
                },
                new Booking
                {
                    Id = Guid.NewGuid(),
                    CheckIn = new DateTime(2024, 5, 15),
                    CheckOut = new DateTime(2024, 5, 18)
                }
            };

            _bookingRepositoryMock
                .Setup(r => r.GetMany(
                    It.IsAny<Expression<Func<Booking, bool>>>(),
                    It.IsAny<Expression<Func<Booking, BookingMinifiedInfoProjection>>>(),
                    It.IsAny<IEnumerable<IOrderClause<Booking>>>()))
                .Returns((Expression<Func<Booking, bool>> filterExpr,
                          Expression<Func<Booking, BookingMinifiedInfoProjection>> selectorExpr,
                          IEnumerable<IOrderClause<Booking>> order) =>
                {
                    var filter = filterExpr.Compile();
                    var selector = selectorExpr.Compile();
                    return bookings.Where(filter).Select(selector);
                });

            var result = _sut.GetAllMinified().ToList();

            result.Should().HaveCount(2);
            result[0].CheckIn.Should().Be(new DateTime(2024, 5, 10));
            result[1].CheckIn.Should().Be(new DateTime(2024, 5, 15));
        }
    }
}