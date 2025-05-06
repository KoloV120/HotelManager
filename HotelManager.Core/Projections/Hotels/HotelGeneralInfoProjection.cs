using System;
using HotelManager.Core.Projections.Rooms;
using HotelManager.Data.Models;

namespace HotelManager.Core.Projections.Hotels;

public record HotelGeneralInfoProjection
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public required string Email { get; set; }

    // Navigation property
    public required ICollection<RoomMinifiedInfoProjection> Rooms { get; set; }

    public required int RoomsPerFloor { get; set; } 
}
