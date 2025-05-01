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

public class HotelService : BaseService<Hotel>, IHotelService
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;

    public HotelService(IRepository<Hotel> repository, IBookingService bookingService, IRoomService roomService)
        : base(repository)
    {
        _bookingService = bookingService;
        _roomService = roomService;
    }

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

    public IEnumerable<HotelMinifiedInfoProjection> GetAllMinified()
    {
        var nameOrderClause = new OrderClause<Hotel> { Expression = a => a.Name };

        return this.Repository.GetMany(
            _ => true,
            a => new HotelMinifiedInfoProjection
            {
                Id = a.Id,
                Name = a.Name
            },
            new[] { nameOrderClause });
    }

    public int GetCurrentGuestsCount(Guid hotelId)
    {
        var bookings = _bookingService.GetAll();
        return bookings.Count(b => 
            b.Room.HotelId == hotelId && 
            b.CheckIn <= DateTime.Now && 
            b.CheckOut >= DateTime.Now);
    }

    public IEnumerable<RoomGeneralInfoProjection> GetAvailableRooms(Guid hotelId)
    {
        var rooms = _roomService.GetAll();
        return rooms.Where(r => 
            r.HotelId == hotelId && 
            !r.Bookings.Any(b => b.CheckOut >= DateTime.Now));
    }

    public IEnumerable<BookingGeneralInfoProjection> GetActiveBookings(Guid hotelId)
    {
        var bookings = _bookingService.GetAll();
        return bookings.Where(b => 
            b.Room.HotelId == hotelId && 
            b.CheckIn <= DateTime.Now && 
            b.CheckOut >= DateTime.Now);
    }

    public decimal GetMonthlyRevenue(Guid hotelId)
    {
        var bookings = _bookingService.GetAll();
        return bookings
            .Where(b => 
                b.Room.HotelId == hotelId && 
                b.CheckIn.Month == DateTime.Now.Month)
            .Sum(b => b.Room.PricePerNight * (b.CheckOut - b.CheckIn).Days);
    }

    public IEnumerable<BookingGeneralInfoProjection> GetRecentBookings(Guid hotelId, int count = 5)
    {
        var bookings = _bookingService.GetAll();
        return bookings
            .Where(b => b.Room.HotelId == hotelId)
            .OrderByDescending(b => b.CheckIn)
            .Take(count)
            .Select(b => new BookingGeneralInfoProjection
            {
                Id = b.Id,
                CheckIn = b.CheckIn,
                CheckOut = b.CheckOut,
                Status = b.Status,
                Guest = new GuestMinifiedInfoProjection
                {
                    Id = b.Guest.Id,
                    Name = b.Guest.Name,
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

    public  HotelDashboardData GetHotelDashboard(Guid hotelId)
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
}
