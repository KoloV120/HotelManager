using System.ComponentModel.DataAnnotations;

namespace HotelManager.Models;

public class BookingInputModel
{
    [Required]
    public Guid GuestId { get; set; }

    [Required]
    public Guid RoomId { get; set; }

    [Required]
    public Guid HotelId { get; set; }

    [Required]
    public DateTime CheckIn { get; set; }

    [Required]
    public DateTime CheckOut { get; set; }
}