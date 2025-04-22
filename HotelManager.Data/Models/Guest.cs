using System;

namespace HotelManager.Data.Models;

public class Guest : IIdentifiable
{
    public Guid Id { get; set; }
}
