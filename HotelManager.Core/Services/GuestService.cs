using System;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Projections.Bookings;
using HotelManager.Core.Projections.Guests;
using HotelManager.Data.Models;
using HotelManager.Data.Repositories;
using HotelManager.Data.Sorting;

namespace HotelManager.Core.Services;

/// <summary>
/// Provides services for managing guests, including retrieving guest information and bookings.
/// </summary>
public class GuestService : BaseService<Guest>, IGuestService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GuestService"/> class.
    /// </summary>
    /// <param name="repository">The guest repository.</param>
    public GuestService(IRepository<Guest> repository) : base(repository)
    {
    }

    /// <summary>
    /// Gets all guests with general information.
    /// </summary>
    /// <returns>A collection of <see cref="GuestGeneralInfoProjection"/>.</returns>
    public IEnumerable<GuestGeneralInfoProjection> GetAll()
    {
        var nameOrderClause = new OrderClause<Guest> { Expression = g => g.Name };

        return this.Repository.GetMany(
            _ => true,
            g => new GuestGeneralInfoProjection
            {
                Id = g.Id,
                Name = g.Name,
                Email = g.Email,
                Phone = g.Phone,
                Bookings = g.Bookings
                    .Select(b => new BookingMinifiedInfoProjection
                    {
                        Id = b.Id,
                        CheckIn = b.CheckIn,
                        CheckOut = b.CheckOut,
                    })
                    .OrderByDescending(b => b.CheckIn)
                    .ToList()
            },
            new[] { nameOrderClause });
    }

    /// <summary>
    /// Gets all guests with minified information.
    /// </summary>
    /// <returns>A collection of <see cref="GuestMinifiedInfoProjection"/>.</returns>
    public IEnumerable<GuestMinifiedInfoProjection> GetAllMinified()
    {
        var nameOrderClause = new OrderClause<Guest> { Expression = g => g.Name };

        return this.Repository.GetMany(
            _ => true,
            g => new GuestMinifiedInfoProjection
            {
                Id = g.Id,
                Name = g.Name,
            },
            new[] { nameOrderClause });
    }

    /// <summary>
    /// Gets all guests who have bookings in a specific hotel.
    /// </summary>
    /// <param name="hotelId">The hotel ID.</param>
    /// <returns>A collection of <see cref="GuestGeneralInfoProjection"/>.</returns>
    public IEnumerable<GuestGeneralInfoProjection> GetAllByHotelId(Guid hotelId) 
    {
        var nameOrderClause = new OrderClause<Guest> { Expression = g => g.Name };

        return this.Repository.GetMany(
            g => g.Bookings.Any(b => b.Room.HotelId == hotelId),
            g => new GuestGeneralInfoProjection
            {
                Id = g.Id,
                Name = g.Name,
                Email = g.Email,
                Phone = g.Phone,
                Bookings = g.Bookings
                    .Where(b => b.Room.HotelId == hotelId)
                    .Select(b => new BookingMinifiedInfoProjection
                    {
                        Id = b.Id,
                        CheckIn = b.CheckIn,
                        CheckOut = b.CheckOut,
                    })
                    .OrderByDescending(b => b.CheckIn)
                    .ToList()
            },
            new[] { nameOrderClause });
    }
}
