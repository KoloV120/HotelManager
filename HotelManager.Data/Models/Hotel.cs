namespace HotelManager.Data.Models;

public class Hotel : IIdentifiable
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public int RoomsPerFloor { get; set; }
}
