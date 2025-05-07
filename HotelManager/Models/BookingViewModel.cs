namespace HotelManager.Models;

public class BookingViewModel
{
    public Guid Id { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public int RoomNumber { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public string Status { get; set; } = string.Empty;
}