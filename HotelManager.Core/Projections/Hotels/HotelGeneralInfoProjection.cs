using System;
using HotelManager.Data.Models;

namespace HotelManager.Core.Projections.Hotels;

public record HotelGeneralInfoProjection
{
    public required Guid Id { get ; set ; }
    public required string Name { get; set;} 
    public required string Address { get; set;} 
    public required string City { get; set;} 
    public required string Email { get; set; } 

    // Navigation property
    public required ICollection<Room> Rooms { get; set; }
}
