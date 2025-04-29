using System;

namespace HotelManager.Core.Projections.Bookings;

public record BookingMinifiedInfoProjection
{
     public required Guid Id { get; set; }
    public required DateTime CheckIn { get; set; }
    public required DateTime CheckOut { get; set; }
}
