using System;

namespace HotelManager.Data.Models;

public interface IIdentifiable
{
    public Guid Id { get; set;}
}
