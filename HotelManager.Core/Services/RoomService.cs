using HotelManager.Core.Interfaces;
using HotelManager.Core.Projections.Bookings;
using HotelManager.Core.Projections.Rooms;
using HotelManager.Data.Models;
using HotelManager.Data.Repositories;
using HotelManager.Data.Sorting;

namespace HotelManager.Core.Services;

/// <summary>
/// Provides services for managing rooms.
/// </summary>
public class RoomService : BaseService<Room>, IRoomService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoomService"/> class.
    /// </summary>
    /// <param name="repository">The repository for room entities.</param>
    public RoomService(IRepository<Room> repository) : base(repository)
    {
    }

    /// <summary>
    /// Gets all rooms with general information.
    /// </summary>
    /// <returns>A collection of <see cref="RoomGeneralInfoProjection"/>.</returns>
    public IEnumerable<RoomGeneralInfoProjection> GetAll()
    {
        var numberOrderClause = new OrderClause<Room> { Expression = r => r.Number };

        return this.Repository.GetMany(
            _ => true,
            r => new RoomGeneralInfoProjection
            {
                Id = r.Id,
                Number = r.Number,
                Type = r.Type,
                PricePerNight = r.PricePerNight,
                Status = r.Status,
                HotelId = r.HotelId,
                Bookings = r.Bookings
                    .Select(b => new BookingMinifiedInfoProjection
                    {
                        Id = b.Id,
                        CheckIn = b.CheckIn,
                        CheckOut = b.CheckOut,
                    })
                    .OrderBy(b => b.CheckIn)
                    .ToList()
            },
            new[] { numberOrderClause });
    }

    /// <summary>
    /// Gets all rooms for a specific hotel by hotel ID.
    /// </summary>
    /// <param name="id">The hotel ID.</param>
    /// <returns>A collection of <see cref="RoomGeneralInfoProjection"/>.</returns>
    public IEnumerable<RoomGeneralInfoProjection> GetAllByHotelId(Guid id)
    {
        var numberOrderClause = new OrderClause<Room> { Expression = r => r.Number };

        return this.Repository.GetMany(
            r => r.HotelId == id,
            r => new RoomGeneralInfoProjection
            {
                Id = r.Id,
                Number = r.Number,
                Type = r.Type,
                PricePerNight = r.PricePerNight,
                Status = r.Status,
                HotelId = r.HotelId,
                Bookings = r.Bookings
                    .Select(b => new BookingMinifiedInfoProjection
                    {
                        Id = b.Id,
                        CheckIn = b.CheckIn,
                        CheckOut = b.CheckOut,
                    })
                    .OrderBy(b => b.CheckIn)
                    .ToList()
            },
            new[] { numberOrderClause }
        );
    }

    /// <summary>
    /// Gets all rooms with minified information.
    /// </summary>
    /// <returns>A collection of <see cref="RoomMinifiedInfoProjection"/>.</returns>
    public IEnumerable<RoomMinifiedInfoProjection> GetAllMinified()
    {
        var numberOrderClause = new OrderClause<Room> { Expression = r => r.Number };

        return this.Repository.GetMany(
            _ => true,
            r => new RoomMinifiedInfoProjection
            {
                Id = r.Id,
                Number = r.Number,
                PricePerNight = r.PricePerNight,
                Status = r.Status,
                HotelId = r.HotelId,
                Type = r.Type
            },
            new[] { numberOrderClause });
    }
    public bool IsRoomCurrentlyBooked(Guid roomId)
    {
        var today = DateTime.Today;

        return this.Repository.GetMany(
            r => r.Id == roomId,
            r => r.Bookings.Any(b => b.CheckIn <= today && b.CheckOut >= today)
        ).Any();
    }

    public void UpdateRoomStatus(Guid roomId)
    {
        var today = DateTime.Today;

        var isBooked = IsRoomCurrentlyBooked(roomId);

        var room = this.Repository.Get(room => room.Id == roomId);
        if (room != null)
        {
            var newStatus = isBooked ? "Booked" : "Available";
            if (room.Status != newStatus)
            {
                room.Status = newStatus;
                this.Repository.Update(room);
            }
        }
    }
}
