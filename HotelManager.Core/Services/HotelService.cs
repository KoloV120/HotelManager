using System;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Projections.Hotels;
using HotelManager.Core.Projections.Rooms;
using HotelManager.Data.Models;
using HotelManager.Data.Repositories;
using HotelManager.Data.Sorting;

namespace HotelManager.Core.Services;

public class HotelService : BaseService<Hotel>, IHotelService
{
     public HotelService(IRepository<Hotel> repository)
            : base(repository)
        {
        }

    public IEnumerable<HotelGeneralInfoProjection> GetAll()
    {
         var nameOrderClause = new OrderClause<Hotel> { Expression = a => a.Name };

            return this.Repository.GetMany(
                _ => true,
                a => new HotelGeneralInfoProjection
                {
                    Id = a.Id,
                    Name = a.Name,
                    Address = a.Address,
                    City = a.City,
                    Email = a.Email,
                    Rooms = a.Rooms
                        .Select(s => new RoomMinifiedInfoProjection
                        {
                            Id = s.Id,
                            Number = s.Number,
                            PricePerNight = s.PricePerNight,
                            HotelId = s.HotelId
                        })
                        .OrderBy(s => s.Number)
                        .ToList()
                },
                new[] { nameOrderClause });
    }

    public IEnumerable<HotelMinifiedInfoProjection> GetAllMinified()
    {
        var nameOrderClause = new OrderClause<Hotel> { Expression = a => a.Name };

            return this.Repository.GetMany(
                _ => true,
                a => new HotelMinifiedInfoProjection
                {
                    Id = a.Id,
                    Name = a.Name
                },
                new[] { nameOrderClause });
    }
}
