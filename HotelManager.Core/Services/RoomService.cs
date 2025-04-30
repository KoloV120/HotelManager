using System;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Projections.Bookings;
using HotelManager.Core.Projections.Rooms;
using HotelManager.Data.Models;
using HotelManager.Data.Repositories;
using HotelManager.Data.Sorting;

namespace HotelManager.Core.Services;

public class RoomService : BaseService<Room>, IRoomService
{
    public RoomService(IRepository<Room> repository) : base(repository)
    {
    }

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

    public IEnumerable<RoomMinifiedInfoProjection> GetAllMinified()
    {
        throw new NotImplementedException();
    }
}
