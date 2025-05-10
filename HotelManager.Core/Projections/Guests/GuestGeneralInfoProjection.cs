using HotelManager.Core.Projections.Bookings;

namespace HotelManager.Core.Projections.Guests;

public record GuestGeneralInfoProjection
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required ICollection<BookingMinifiedInfoProjection> Bookings { get; set; }
}
