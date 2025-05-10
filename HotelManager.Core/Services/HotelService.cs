using System;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Projections.Bookings;
using HotelManager.Core.Projections.Guests;
using HotelManager.Core.Projections.Hotels;
using HotelManager.Core.Projections.Rooms;
using HotelManager.Data.Models;
using HotelManager.Data.Repositories;
using HotelManager.Data.Sorting;

namespace HotelManager.Core.Services;

/// <summary>
/// Provides services for managing hotels, including retrieving hotel information,
/// bookings, rooms, and calculating statistics.
/// </summary>
public class HotelService : BaseService<Hotel>, IHotelService
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HotelService"/> class.
    /// </summary>
    /// <param name="repository">The hotel repository.</param>
    /// <param name="bookingService">The booking service.</param>
    /// <param name="roomService">The room service.</param>
    public HotelService(IRepository<Hotel> repository, IBookingService bookingService, IRoomService roomService)
        : base(repository)
    {
        _bookingService = bookingService;
        _roomService = roomService;
    }

    /// <summary>
    /// Gets all hotels with general information.
    /// </summary>
    /// <returns>A collection of <see cref="HotelGeneralInfoProjection"/>.</returns>
    public IEnumerable<HotelGeneralInfoProjection> GetAll()
    {
        var nameOrderClause = new OrderClause<Hotel> { Expression = a => a.Name };

        return this.Repository.GetMany(
            _ => true,
            a => new HotelGeneralInfoProjection
            {
                Id = a.Id,
                Name = a.Name,
                Address = a.Address,
                City = a.City,
                Email = a.Email,
                RoomsPerFloor = a.RoomsPerFloor,
                Rooms = a.Rooms
                    .Select(s => new RoomMinifiedInfoProjection
                    {
                        Id = s.Id,
                        Number = s.Number,
                        PricePerNight = s.PricePerNight,
                        HotelId = s.HotelId,
                        Status = s.Status,
                        Type = s.Type
                    })
                    .OrderBy(s => s.Number)
                    .ToList()
            },
            new[] { nameOrderClause });
    }

    /// <summary>
    /// Gets all hotels with minified information.
    /// </summary>
    /// <returns>A collection of <see cref="HotelMinifiedInfoProjection"/>.</returns>
    public IEnumerable<HotelMinifiedInfoProjection> GetAllMinified()
    {
        var nameOrderClause = new OrderClause<Hotel> { Expression = a => a.Name };

        return this.Repository.GetMany(
            _ => true,
            a => new HotelMinifiedInfoProjection
            {
                Id = a.Id,
                Name = a.Name,
                Rooms = a.Rooms
                    .Select(s => new RoomMinifiedInfoProjection
                    {
                        Id = s.Id,
                        Number = s.Number,
                        PricePerNight = s.PricePerNight,
                        HotelId = s.HotelId,
                        Status = s.Status,
                        Type = s.Type
                    })
                    .OrderBy(s => s.Number)
                    .ToList()
            },
            new[] { nameOrderClause });
    }

    /// <summary>
    /// Gets the current number of guests in a hotel.
    /// </summary>
    /// <param name="hotelId">The hotel ID.</param>
    /// <returns>The number of current guests.</returns>
    public int GetCurrentGuestsCount(Guid hotelId)
    {
        var bookings = this.GetActiveBookings(hotelId);
        int currentGuestsCount = 0;
        foreach (var booking in bookings)
        {
           if(booking.Room.Type == "Single")
           {
               currentGuestsCount += 1;
           }
           else if(booking.Room.Type == "Double")
           {
               currentGuestsCount += 2;
           }
           else if(booking.Room.Type == "Suite")
           {
               currentGuestsCount += 4;
           }
        }
        return currentGuestsCount;
    }

    /// <summary>
    /// Gets all available rooms for a specific hotel.
    /// </summary>
    /// <param name="hotelId">The hotel ID.</param>
    /// <returns>A collection of available <see cref="RoomGeneralInfoProjection"/>.</returns>
    public IEnumerable<RoomGeneralInfoProjection> GetAvailableRooms(Guid hotelId)
    {
        var bookings = this.GetActiveBookings(hotelId);
        List<Guid> bookedRoomIds = bookings.Select(b => b.Room.Id).ToList();
        var allRooms = _roomService.GetAllByHotelId(hotelId);
        return allRooms.Where(r => 
            bookedRoomIds.Contains(r.Id) == false &&
            r.Status == "Available");
    }

    /// <summary>
    /// Gets all active bookings for a specific hotel.
    /// </summary>
    /// <param name="hotelId">The hotel ID.</param>
    /// <returns>A collection of <see cref="BookingGeneralInfoProjection"/>.</returns>
    public IEnumerable<BookingGeneralInfoProjection> GetActiveBookings(Guid hotelId)
    {
        var bookings = _bookingService.GetAll();
        return bookings.Where(b => 
            b.Room.HotelId == hotelId && 
            b.CheckIn <= DateTime.Now && 
            b.CheckOut >= DateTime.Now);
    }

    /// <summary>
    /// Gets the total monthly revenue for a specific hotel.
    /// </summary>
    /// <param name="hotelId">The hotel ID.</param>
    /// <returns>The monthly revenue as a decimal.</returns>
    public decimal GetMonthlyRevenue(Guid hotelId)
    {
        var bookings = _bookingService.GetAll();
        return bookings
            .Where(b => 
                b.Room.HotelId == hotelId && 
                b.CheckIn.Month == DateTime.Now.Month)
            .Sum(b => b.Room.PricePerNight * (b.CheckOut - b.CheckIn).Days);
    }

    /// <summary>
    /// Gets the most recent bookings for a specific hotel.
    /// </summary>
    /// <param name="hotelId">The hotel ID.</param>
    /// <param name="count">The number of recent bookings to retrieve. Default is 5.</param>
    /// <returns>A collection of <see cref="RecentBookingProjection"/>.</returns>
    public IEnumerable<RecentBookingProjection> GetRecentBookings(Guid hotelId, int count = 5)
    {
        var bookings = _bookingService.GetAll();
        return bookings
            .Where(b => b.Room.HotelId == hotelId)
            .OrderByDescending(b => b.CheckIn)
            .Take(count)
            .Select(b => new RecentBookingProjection
            {
                Id = b.Id,
                CheckIn = b.CheckIn,
                CheckOut = b.CheckOut,
                GuestName = b.Guest.Name,
                RoomNumber = b.Room.Number
            });
    }

    /// <summary>
    /// Gets dashboard data for a specific hotel, including statistics and recent activity.
    /// </summary>
    /// <param name="hotelId">The hotel ID.</param>
    /// <returns>A <see cref="HotelDashboardData"/> object with hotel statistics.</returns>
    public HotelDashboardData GetHotelInfo(Guid hotelId)
    {
        var hotel =  GetById(hotelId);
        if (hotel == null)
            throw new InvalidOperationException($"Hotel with ID {hotelId} not found");

        return new HotelDashboardData
        {
            HotelName = hotel.Name,
            TotalGuests =  GetCurrentGuestsCount(hotelId),
            AvailableRooms =  GetAvailableRooms(hotelId),
            ActiveBookings =  GetActiveBookings(hotelId),
            MonthlyRevenue =  GetMonthlyRevenue(hotelId),
            RecentBookings =  GetRecentBookings(hotelId)
        };
    }

    /// <summary>
    /// Gets the number of rooms per floor for a specific hotel.
    /// </summary>
    /// <param name="hotelId">The hotel ID.</param>
    /// <returns>The number of rooms per floor.</returns>
    public int GetRoomsPerFloor(Guid hotelId)
    {
        var hotel = GetById(hotelId);
        if (hotel == null)
            throw new InvalidOperationException($"Hotel with ID {hotelId} not found");

        return hotel.RoomsPerFloor;
    }

    /// <summary>
    /// Gets all rooms for a specific hotel.
    /// </summary>
    /// <param name="hotelId">The hotel ID.</param>
    /// <returns>A collection of <see cref="RoomGeneralInfoProjection"/>.</returns>
    public IEnumerable<RoomGeneralInfoProjection> GetAllRooms(Guid hotelId) 
    {
        var rooms = _roomService.GetAll();
        return rooms.Where(r => 
            r.HotelId == hotelId);
    }

    /// <summary>
    /// Gets all bookings for a specific hotel.
    /// </summary>
    /// <param name="hotelId">The hotel ID.</param>
    /// <returns>A collection of <see cref="BookingGeneralInfoProjection"/>.</returns>
    public IEnumerable<BookingGeneralInfoProjection> GetAllBookings(Guid hotelId) 
    {
        var bookings = _bookingService.GetAll();
        return bookings
            .Where(b => b.Room.HotelId == hotelId)
            .OrderByDescending(b => b.CheckIn)
            .Select(b => new BookingGeneralInfoProjection
            {
                Id = b.Id,
                CheckIn = b.CheckIn,
                CheckOut = b.CheckOut,
                Status = b.Status,
                Guest = new GuestMinifiedInfoProjection
                {
                    Id = b.Guest.Id,
                    Name = b.Guest.Name
                },
                Room = new RoomMinifiedInfoProjection
                {
                    Id = b.Room.Id,
                    Number = b.Room.Number,
                    PricePerNight = b.Room.PricePerNight,
                    HotelId = b.Room.HotelId,
                    Status = b.Room.Status,
                    Type = b.Room.Type
                }
            });
    }
}
