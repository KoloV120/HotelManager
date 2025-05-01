using HotelManager.Core.Projections.Bookings;
using HotelManager.Core.Projections.Rooms;
using HotelManager.Models;

public class HotelDashboardData
{
    public required string HotelName { get; set; }
    public required int TotalGuests { get; set; }
    public required IEnumerable<RoomGeneralInfoProjection> AvailableRooms { get; set; }
    public required IEnumerable<BookingGeneralInfoProjection> ActiveBookings { get; set; }
    public required decimal MonthlyRevenue { get; set; }
    public required IEnumerable<RecentBookingInfo> RecentBookings { get; set; }
}