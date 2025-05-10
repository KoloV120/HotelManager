using HotelManager.Core.Interfaces;
using HotelManager.Core.Projections.Bookings;
using HotelManager.Core.Projections.Guests;
using HotelManager.Core.Projections.Rooms;
using HotelManager.Data.Models;
using HotelManager.Data.Repositories;
using HotelManager.Data.Sorting;

namespace HotelManager.Core.Services;

/// <summary>
/// Provides services for managing bookings, including retrieving booking information and checking room availability.
/// </summary>
public class BookingService : BaseService<Booking>, IBookingService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookingService"/> class.
    /// </summary>
    /// <param name="repository">The booking repository.</param>
    public BookingService(IRepository<Booking> repository) : base(repository)
    {
    }

    /// <summary>
    /// Gets all bookings with general information.
    /// </summary>
    /// <returns>A collection of <see cref="BookingGeneralInfoProjection"/>.</returns>
    public IEnumerable<BookingGeneralInfoProjection> GetAll()
    {
        var checkInOrderClause = new OrderClause<Booking> { Expression = b => b.CheckIn };

        return this.Repository.GetMany(
            _ => true,
            b => new BookingGeneralInfoProjection
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
                    PricePerNight = b.Room.PricePerNight,
                    HotelId = b.Room.HotelId,
                    Status = b.Room.Status,
                    Type = b.Room.Type
                }
            },
            new[] { checkInOrderClause });
    }

    /// <summary>
    /// Gets all bookings with minified information.
    /// </summary>
    /// <returns>A collection of <see cref="BookingMinifiedInfoProjection"/>.</returns>
    public IEnumerable<BookingMinifiedInfoProjection> GetAllMinified()
    {
        var checkInOrderClause = new OrderClause<Booking> { Expression = b => b.CheckIn };

        return this.Repository.GetMany(
            _ => true,
            b => new BookingMinifiedInfoProjection
            {
                Id = b.Id,
                CheckIn = b.CheckIn,
                CheckOut = b.CheckOut,
            },
            new[] { checkInOrderClause });
    }

    /// <summary>
    /// Checks if a room is available for the specified period.
    /// </summary>
    /// <param name="roomId">The room ID.</param>
    /// <param name="checkIn">The check-in date.</param>
    /// <param name="checkOut">The check-out date.</param>
    /// <returns><c>true</c> if the room is available; otherwise, <c>false</c>.</returns>
    public bool IsRoomAvailable(Guid roomId, DateTime checkIn, DateTime checkOut)
    {
        var conflictingBooking = GetAll()
        .FirstOrDefault(b => b.Room.Id == roomId &&
                             checkIn < b.CheckOut &&
                             checkOut > b.CheckIn);

        // Return true if no conflicting booking is found
        return conflictingBooking == null;
    }
}

