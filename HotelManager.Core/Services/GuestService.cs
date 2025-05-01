using System;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Projections.Bookings;
using HotelManager.Core.Projections.Guests;
using HotelManager.Data.Models;
using HotelManager.Data.Repositories;
using HotelManager.Data.Sorting;

namespace HotelManager.Core.Services;

public class GuestService : BaseService<Guest>, IGuestService
{
    public GuestService(IRepository<Guest> repository) : base(repository)
    {
    }

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
}
