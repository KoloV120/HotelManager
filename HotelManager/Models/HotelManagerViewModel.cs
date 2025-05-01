namespace HotelManager.Models;

public class HotelManagerViewModel
{
    public int TotalGuests { get; set; }
    public int AvailableRooms { get; set; }
    public int ActiveBookings { get; set; }
    public decimal MonthlyRevenue { get; set; }

    public List<RecentBookingInfo> RecentBookings { get; set; } = new();
    public List<GuestSelectListItem> AvailableGuests { get; set; } = new();
    public List<RoomSelectListItem> ListOfAvailableRooms { get; set; } = new();
}

public class RecentBookingInfo
{
    public string? GuestName { get; set; }
    public int RoomNumber { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
}

public class GuestSelectListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class RoomSelectListItem
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public decimal PricePerNight { get; set; }
}