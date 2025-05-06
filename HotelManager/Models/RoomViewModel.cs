namespace HotelManager.Models;

public class RoomViewModel
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public string Status { get; set; } = string.Empty;
}