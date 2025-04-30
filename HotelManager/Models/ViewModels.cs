namespace HotelManager.Models;

public class DashboardViewModel
{
    public int TotalGuests { get; set; }
    public int AvailableRooms { get; set; }
    public int ActiveBookings { get; set; }
    public decimal MonthlyRevenue { get; set; }

    public List<RecentBookingInfo> RecentBookings { get; set; } = new();
}

public class RecentBookingInfo
{
    public string? GuestName { get; set; }
    public string? RoomNumber { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
}
