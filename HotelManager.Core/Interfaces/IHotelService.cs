using System;
using HotelManager.Core.Projections.Bookings;
using HotelManager.Core.Projections.Hotels;
using HotelManager.Core.Projections.Rooms;
using HotelManager.Data.Models;
using HotelManager.Models;

namespace HotelManager.Core.Interfaces;

public interface IHotelService : IService<Hotel>
{
    IEnumerable<HotelGeneralInfoProjection> GetAll();
    IEnumerable<HotelMinifiedInfoProjection> GetAllMinified();
   int GetCurrentGuestsCount(Guid hotelId);
    IEnumerable<RoomGeneralInfoProjection> GetAvailableRooms(Guid hotelId);
    IEnumerable<BookingGeneralInfoProjection> GetActiveBookings(Guid hotelId);
    decimal GetMonthlyRevenue(Guid hotelId);
    IEnumerable<RecentBookingInfo> GetRecentBookings(Guid hotelId, int count = 5);
    HotelDashboardData GetHotelInfo(Guid hotelId);
}
