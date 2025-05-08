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

        [Fact]
        public void IsRoomAvailable_NoConflictingBookings_ReturnsTrue()
        {
            // Arrange
            var roomId = Guid.NewGuid();
            var checkIn = DateTime.Today.AddDays(1);
            var checkOut = DateTime.Today.AddDays(3);

            _bookingRepositoryMock.Setup(x => x.GetMany(
                It.IsAny<System.Linq.Expressions.Expression<Func<Booking, bool>>>()))
                .Returns(new List<Booking>());

            // Act
            var result = _sut.IsRoomAvailable(roomId, checkIn, checkOut);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Create_ValidBooking_ReturnsTrue()
        {
            // Arrange
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                CheckIn = DateTime.Today.AddDays(1),
                CheckOut = DateTime.Today.AddDays(3),
                Status = "confirmed"
            };

            _bookingRepositoryMock.Setup(x => x.Create(It.IsAny<Booking>()));

            // Act
            var result = _sut.Create(booking);

            // Assert
            result.Should().BeTrue();
            _bookingRepositoryMock.Verify(x => x.Create(booking), Times.Once);
        }

        [Fact]
        public void GetAll_ShouldReturnAllBookings()
        {
            // Arrange
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
                        Type = "Standard",
                        PricePerNight = 100,
                        HotelId = Guid.NewGuid()
                    }
                }));

            // Act
            var result = _sut.GetAll();

            // Assert
            result.Should().NotBeEmpty();
            result.First().Guest.Should().NotBeNull();
            result.First().Room.Should().NotBeNull();
        }

        [Theory]
        [InlineData(true)]  // No conflicting bookings
        [InlineData(false)] // Has conflicting bookings
        public void IsRoomAvailable_ShouldReturnCorrectAvailability(bool expectedAvailability)
        {
            // Arrange
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
                            Type = "Standard",
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

            // Act
            var result = _sut.IsRoomAvailable(roomId, checkIn, checkOut);

            // Assert
            result.Should().Be(expectedAvailability);
        }
        [Fact]
        public void GetAllMinified_ShouldReturnAllBookingsMinified()
        {
            // Arrange
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

            // Act
            var result = _sut.GetAllMinified().ToList();

            // Assert
            result.Should().HaveCount(2);
            result[0].CheckIn.Should().Be(new DateTime(2024, 5, 10));
            result[1].CheckIn.Should().Be(new DateTime(2024, 5, 15));
        }
    }
}