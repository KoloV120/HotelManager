using System;

namespace HotelManager.Core.Projections.Guests;

public record GuestMinifiedInfoProjection
{
     public  required Guid Id { get; set; }
    public required string Name { get; set; }
}
