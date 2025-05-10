namespace HotelManager.Core.Projections.Hotels;

using HotelManager.Core.Projections.Rooms;

public record HotelMinifiedInfoProjection
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required ICollection<RoomMinifiedInfoProjection> Rooms { get; set; }
}
