using System;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Projections.Rooms;
using HotelManager.Data.Models;

namespace HotelManager.Core.Configuration;

public interface IRoomService : IService<Room>
{
    IEnumerable<RoomGeneralInfoProjection> GetAll();
    IEnumerable<RoomMinifiedInfoProjection> GetAllMinified();
}