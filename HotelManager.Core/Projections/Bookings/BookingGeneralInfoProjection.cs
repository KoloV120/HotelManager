using HotelManager.Core.Projections.Guests;
using HotelManager.Core.Projections.Rooms;

namespace HotelManager.Core.Projections.Bookings;

public record BookingGeneralInfoProjection
{
  public required Guid Id { get; set; }
  public required DateTime CheckIn { get; set; }
  public required DateTime CheckOut { get; set; }
  public required string Status { get; set; }
  public required GuestMinifiedInfoProjection Guest { get; set; }
  public required RoomMinifiedInfoProjection Room { get; set; }
}
