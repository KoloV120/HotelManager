using HotelManager.Core.Projections.Guests;
using HotelManager.Data.Models;

namespace HotelManager.Core.Interfaces;

public interface IGuestService : IService<Guest>
{
    IEnumerable<GuestGeneralInfoProjection> GetAll();

    IEnumerable<GuestMinifiedInfoProjection> GetAllMinified();

    IEnumerable<GuestGeneralInfoProjection> GetAllByHotelId(Guid hotelId);
}
