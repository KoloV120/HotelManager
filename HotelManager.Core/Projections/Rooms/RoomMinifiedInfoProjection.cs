using System;

namespace HotelManager.Core.Projections.Rooms;

public record RoomMinifiedInfoProjection
{
    public required Guid Id { get  ; set ; }
    public required int Number { get; set; } 
    public required decimal PricePerNight { get; set; }
}
