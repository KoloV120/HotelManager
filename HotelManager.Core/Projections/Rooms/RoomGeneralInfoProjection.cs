using System;
using HotelManager.Core.Projections.Bookings;

namespace HotelManager.Core.Projections.Rooms;

public record RoomGeneralInfoProjection
{
    public required Guid Id { get; set; }
    public required int Number { get; set; }
    public required string Type { get; set; }
    public required decimal PricePerNight { get; set; }
    public required string Status { get; set; }
    public required  ICollection<BookingMinifiedInfoProjection> Bookings { get; set; }
}
