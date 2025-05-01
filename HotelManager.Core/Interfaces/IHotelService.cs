using System;
using HotelManager.Core.Projections.Bookings;
using HotelManager.Core.Projections.Hotels;
using HotelManager.Data.Models;

namespace HotelManager.Core.Interfaces;

public interface IHotelService : IService<Hotel>
{
    IEnumerable<HotelGeneralInfoProjection> GetAll();
    IEnumerable<HotelMinifiedInfoProjection> GetAllMinified();
   int GetCurrentGuestsCount(Guid hotelId);
    int GetAvailableRoomsCount(Guid hotelId);
    int GetActiveBookingsCount(Guid hotelId);
    decimal GetMonthlyRevenue(Guid hotelId);
    IEnumerable<BookingGeneralInfoProjection> GetRecentBookings(Guid hotelId, int count = 5);
    HotelDashboardData GetHotelDashboard(Guid hotelId);
}
