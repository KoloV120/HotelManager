using System;

namespace HotelManager.Data.Models;

public class Room : IIdentifiable
{
    public Guid Id { get  ; set ; }
    public int Number { get; set; } 
    public string Type { get; set; } = string.Empty;   // "single", "double", "suite"

    public decimal PricePerNight { get; set; }

    public string Status { get; set; }  = string.Empty;// "available", "booked", "maintenance"

    // Foreign key
    //public int HotelId { get; set; }

    // Navigation properties
    public Hotel Hotel { get; set; } = new Hotel();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
