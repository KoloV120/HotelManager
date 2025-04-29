using System;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Projections.Bookings;
using HotelManager.Core.Projections.Guests;
using HotelManager.Core.Projections.Rooms;
using HotelManager.Data.Models;
using HotelManager.Data.Repositories;
using HotelManager.Data.Sorting;

namespace HotelManager.Core.Services;

public class BookingService : BaseService<Booking>, IBookingService
{
    public BookingService(IRepository<Booking> repository) : base(repository)
    {
    }

    public IEnumerable<BookingGeneralInfoProjection> GetAll()
    {
        var checkInOrderClause = new OrderClause<Booking> { Expression = b => b.CheckIn };

        return this.Repository.GetMany(
            _ => true,
            b => new BookingGeneralInfoProjection
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
                    Number = b.Room.Number
                }
            },
            new[] { checkInOrderClause });
    }

    public IEnumerable<BookingMinifiedInfoProjection> GetAllMinified()
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsRoomAvailable(Guid roomId, DateTime checkIn, DateTime checkOut)
    {
        throw new NotImplementedException();
    }
    }

