using System;

namespace HotelManager.Data.Models;

public class Hotels : IIdentifiable
{
    public Guid Id { get ; set ; }
    public string Name { get; set;}
    public string Address { get; set;}
    public string City { get; set;}
    

}
