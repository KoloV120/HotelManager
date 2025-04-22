using System;

namespace HotelManager.Data.Models;

public class Guest : IIdentifiable
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    // Navigation property
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
