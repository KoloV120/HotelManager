namespace HotelManager.Data.Models;

public class Booking : IIdentifiable
{
    public Guid Id { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guest Guest { get; set; } = new Guest();
    public Room Room { get; set; } = new Room();
}
