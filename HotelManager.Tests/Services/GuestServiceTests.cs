using Xunit;
using Moq;
using FluentAssertions;
using HotelManager.Core.Services;
using HotelManager.Data.Models;
using HotelManager.Data.Repositories;
using HotelManager.Core.Projections.Guests;
using HotelManager.Data.Sorting;
using HotelManager.Core.Projections.Bookings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace HotelManager.Tests.Services
{
    public class GuestServiceTests
    {
        private readonly Mock<IRepository<Guest>> _guestRepositoryMock;
        private readonly GuestService _sut;

        public GuestServiceTests()
        {
            _guestRepositoryMock = new Mock<IRepository<Guest>>();
            _sut = new GuestService(_guestRepositoryMock.Object);
        }

        /// <summary>
        /// Tests the <see cref="GuestService.Create"/> method to ensure it successfully creates a valid guest.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method calls the repository's <see cref="IRepository{T}.Create"/> method with the correct guest object.
        /// It mocks the repository to simulate the creation process and checks that the method returns <c>true</c> upon successful creation.
        /// The test also ensures that the repository's <see cref="IRepository{T}.Create"/> method is called exactly once with the expected guest object.
        /// </remarks>
        [Fact]
        public void Create_ValidGuest_ReturnsTrue()
        {
            var guest = new Guest
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john@example.com",
                Phone = "1234567890"
            };

            _guestRepositoryMock.Setup(x => x.Create(guest));

            var result = _sut.Create(guest);

            result.Should().BeTrue();
            _guestRepositoryMock.Verify(x => x.Create(guest), Times.Once);
        }

        /// <summary>
        /// Tests the <see cref="GuestService.GetById"/> method to ensure it returns the correct guest when the specified guest ID exists.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method retrieves the correct guest for the provided ID.
        /// It mocks the <see cref="IRepository{Guest}.Get"/> method to return a predefined guest object.
        /// The test ensures that the result is not null and that the returned guest's ID matches the provided ID.
        /// </remarks>
        [Fact]
        public void GetById_ExistingGuest_ReturnsGuest()
        {
            var guestId = Guid.NewGuid();
            var guest = new Guest { Id = guestId, Name = "John Doe" };

            _guestRepositoryMock.Setup(x => x.Get(It.IsAny<System.Linq.Expressions.Expression<Func<Guest, bool>>>()))
                .Returns(guest);

            var result = _sut.GetById(guestId);

            result.Should().NotBeNull();
            result.Id.Should().Be(guestId);
        }

        /// <summary>
        /// Tests the <see cref="GuestService.GetAll"/> method to ensure it returns all guests with their general information.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method retrieves all guests and maps them to the <see cref="GuestGeneralInfoProjection"/> format.
        /// It mocks the <see cref="IRepository{Guest}.GetMany"/> method to return a predefined list of guests.
        /// The test ensures that the result is not empty, contains the expected number of guests, and matches the predefined list of guests.
        /// </remarks>
        [Fact]
        public void GetAll_ReturnsAllGuests()
        {
            var guests = new List<GuestGeneralInfoProjection>
            {
                new GuestGeneralInfoProjection 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "John Doe",
                    Email = "john@example.com",
                    Phone = "1234567890",
                    Bookings = new List<BookingMinifiedInfoProjection>()
                },
                new GuestGeneralInfoProjection 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Jane Doe",
                    Email = "jane@example.com",
                    Phone = "0987654321",
                    Bookings = new List<BookingMinifiedInfoProjection>()
                }
            };

            _guestRepositoryMock
                .Setup(x => x.GetMany(
                    It.IsAny<Expression<Func<Guest, bool>>>(),
                    It.IsAny<Expression<Func<Guest, GuestGeneralInfoProjection>>>(),
                    It.IsAny<IEnumerable<IOrderClause<Guest>>>()))
                .Returns(guests);

            var result = _sut.GetAll();

            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(guests, options => options
                .Including(x => x.Id)
                .Including(x => x.Name)
                .Including(x => x.Email)
                .Including(x => x.Phone)
                .Including(x => x.Bookings));
        }

        /// <summary>
        /// Tests the <see cref="GuestService.GetAllMinified"/> method to ensure it returns a collection of minified guest information.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method retrieves all guests and maps them to the <see cref="GuestMinifiedInfoProjection"/> format.
        /// It mocks the <see cref="IRepository{Guest}.GetMany"/> method to return a predefined list of guests.
        /// The test ensures that the result is not empty and contains the expected minified guest information.
        /// </remarks>
        [Fact]
        public void GetAllMinified_ShouldReturnMinifiedGuestInfo()
        {
            var guests = new List<Guest>
            {
                new Guest
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "john@example.com",
                    Phone = "1234567890"
                }
            };

            _guestRepositoryMock
                .Setup(x => x.GetMany(
                    It.IsAny<Expression<Func<Guest, bool>>>(),
                    It.IsAny<Expression<Func<Guest, GuestMinifiedInfoProjection>>>(),
                    It.IsAny<IEnumerable<IOrderClause<Guest>>>()))
                .Returns(guests.Select(g => new GuestMinifiedInfoProjection
                {
                    Id = g.Id,
                    Name = g.Name
                }));

            var result = _sut.GetAllMinified();

            result.Should().NotBeEmpty();
            var guest = result.First();
            guest.Name.Should().Be("John Doe");
        }

        /// <summary>
        /// Tests the <see cref="GuestService.GetById"/> method to ensure it returns <c>null</c> when the specified guest ID does not exist.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method correctly handles the case where no guest is found for the provided ID.
        /// It mocks the <see cref="IRepository{Guest}.Get"/> method to return <c>null</c> and ensures the result is <c>null</c>.
        /// </remarks>
        [Fact]
        public void GetById_NonExistingGuest_ReturnsNull()
        {
            var guestId = Guid.NewGuid();
            _guestRepositoryMock
                .Setup(x => x.Get(It.IsAny<Expression<Func<Guest, bool>>>()))
                .Returns((Guest)null);

            var result = _sut.GetById(guestId);

            result.Should().BeNull();
        }

        /// <summary>
        /// Tests the <see cref="GuestService.GetAllByHotelId"/> method to ensure it returns guests with bookings for the specified hotel ID.
        /// </summary>
        /// <remarks>
        /// This test verifies that the method filters guests by the provided hotel ID and retrieves only those guests who have bookings associated with that hotel.
        /// It mocks the <see cref="IRepository{Guest}.GetMany"/> method to return a predefined list of guests with bookings.
        /// The test ensures that the result contains only the guests with bookings for the specified hotel and verifies the booking details.
        /// </remarks>
        [Fact]
        public void GetAllByHotelId_ShouldReturnGuestsWithBookingsForThatHotel()
        {
            var hotelId = Guid.NewGuid();
            var guestId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var guests = new List<Guest>
            {
                new Guest
                {
                    Id = guestId,
                    Name = "Alice",
                    Email = "alice@example.com",
                    Phone = "1234567890",
                    Bookings = new List<Booking>
                    {
                        new Booking
                        {
                            Id = bookingId,
                            CheckIn = DateTime.Today,
                            CheckOut = DateTime.Today.AddDays(2),
                            Room = new Room
                            {
                                Id = Guid.NewGuid(),
                                HotelId = hotelId
                            }
                        }
                    }
                },
                new Guest
                {
                    Id = Guid.NewGuid(),
                    Name = "Bob",
                    Email = "bob@example.com",
                    Phone = "0987654321",
                    Bookings = new List<Booking>
                    {
                        new Booking
                        {
                            Id = Guid.NewGuid(),
                            CheckIn = DateTime.Today,
                            CheckOut = DateTime.Today.AddDays(1),
                            Room = new Room
                            {
                                Id = Guid.NewGuid(),
                                HotelId = Guid.NewGuid() 
                            }
                        }
                    }
                }
            };

            _guestRepositoryMock
    .Setup(r => r.GetMany(
        It.IsAny<Expression<Func<Guest, bool>>>(),
        It.IsAny<Expression<Func<Guest, GuestGeneralInfoProjection>>>(),
        It.IsAny<IEnumerable<IOrderClause<Guest>>>()))
    .Returns((Expression<Func<Guest, bool>> filterExpr,
              Expression<Func<Guest, GuestGeneralInfoProjection>> selectorExpr,
              IEnumerable<IOrderClause<Guest>> order) =>
    {
        var filter = filterExpr.Compile();
        var selector = selectorExpr.Compile();
        return guests.Where(filter).Select(selector);
    });
            var result = _sut.GetAllByHotelId(hotelId).ToList();

            result.Should().HaveCount(1);
            var guest = result.First();
            guest.Name.Should().Be("Alice");
            guest.Bookings.Should().HaveCount(1);
            guest.Bookings.First().Id.Should().Be(bookingId);
        }
    }
}