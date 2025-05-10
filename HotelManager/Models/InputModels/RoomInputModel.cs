using System.ComponentModel.DataAnnotations;

namespace HotelManager.Data.Models;

public class RoomInputModel
{
    public Guid Id { get; set; }

    [Required]
    public int Number { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    public decimal PricePerNight { get; set; }

    public string Status { get; set; } = string.Empty;

    public Guid HotelId { get; set; }

    public Hotel Hotel { get; set; } = new Hotel();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}