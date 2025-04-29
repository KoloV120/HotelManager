using System;
using HotelManager.Data.Models;

namespace HotelManager.Core.Projections.Hotels;

public record HotelMinifiedInfoProjection
{
     public required Guid Id { get ; set ; }
    public required string Name { get; set;} 
    //public required ICollection<Room> Rooms { get; set; }
}
