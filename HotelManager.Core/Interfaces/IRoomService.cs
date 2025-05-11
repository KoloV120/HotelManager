using HotelManager.Core.Projections.Rooms;
using HotelManager.Data.Models;

namespace HotelManager.Core.Interfaces;

public interface IRoomService : IService<Room>
{
    IEnumerable<RoomGeneralInfoProjection> GetAll();

    IEnumerable<RoomMinifiedInfoProjection> GetAllMinified();

    IEnumerable<RoomGeneralInfoProjection> GetAllByHotelId(Guid id);

    public bool IsRoomCurrentlyBooked(Guid roomId);

    public void UpdateRoomStatus(Guid roomId);
}
