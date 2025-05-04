using System;
using HotelManager.Core.Projections.Bookings;
using HotelManager.Data.Models;

namespace HotelManager.Core.Interfaces;

public interface IBookingService : IService<Booking>
{
    IEnumerable<BookingGeneralInfoProjection> GetAll();
    IEnumerable<BookingMinifiedInfoProjection> GetAllMinified();
    bool IsRoomAvailable(Guid roomId, DateTime checkIn, DateTime checkOut);
}
