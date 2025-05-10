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