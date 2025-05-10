namespace HotelManager.Data.Models;

public class Room : IIdentifiable
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid HotelId { get; set; }
    public Hotel? Hotel { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
