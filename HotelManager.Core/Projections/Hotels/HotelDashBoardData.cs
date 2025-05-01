using HotelManager.Core.Projections.Bookings;

public class HotelDashboardData
{
    public string? HotelName { get; set; }
    public int TotalGuests { get; set; }
    public int AvailableRooms { get; set; }
    public int ActiveBookings { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public IEnumerable<BookingGeneralInfoProjection>? RecentBookings { get; set; }
}