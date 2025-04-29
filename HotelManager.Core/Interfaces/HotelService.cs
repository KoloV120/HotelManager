using System;
using HotelManager.Core.Projections.Hotels;
using HotelManager.Data.Models;

namespace HotelManager.Core.Interfaces;

public interface IHotelService : IService<Hotel>
{
        IEnumerable<HotelGeneralInfoProjection> GetAll();
        IEnumerable<HotelMinifiedInfoProjection> GetAllMinified();
}
