namespace HotelManager.Core.Projections.Bookings;

public record RecentBookingProjection
{
    public required Guid Id { get; set; }
    public required DateTime CheckIn { get; set; }
    public required DateTime CheckOut { get; set; }
    public string? GuestName { get; set; }
    public int RoomNumber { get; set; }
}