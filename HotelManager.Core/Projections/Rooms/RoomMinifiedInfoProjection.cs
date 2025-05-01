using System;
using HotelManager.Core.Projections.Bookings;
using HotelManager.Core.Projections.Hotels;

namespace HotelManager.Core.Projections.Rooms;

public record RoomMinifiedInfoProjection
{
    public required Guid Id { get  ; set ; }
    public required int Number { get; set; } 
    public required decimal PricePerNight { get; set; }
    public required string Status { get; set; }
    public required string Type { get; set; }
    public required Guid HotelId { get; set; } // Foreign key
}
