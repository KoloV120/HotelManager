using Microsoft.AspNetCore.Mvc;
using HotelManager.Core.Interfaces;
using HotelManager.Models;
using HotelManager.Data.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HotelManager.Controllers;

public class BookingController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;
    private readonly IGuestService _guestService;
    private readonly IHotelService _hotelService;
    private readonly ILogger<BookingController> _logger;

    public BookingController(IBookingService bookingService, IRoomService roomService, IGuestService guestService, IHotelService hotelService, ILogger<BookingController> logger)
    {
        _bookingService = bookingService;
        _roomService = roomService;
        _guestService = guestService;
        _hotelService = hotelService;
        _logger = logger;
    }

    public IActionResult Index(Guid id)
    {
        try
        {
            ViewData["HotelId"] = id;

            var bookings = _hotelService.GetAllBookings(id)
            .Select(b => new BookingViewModel
            {
                Id = b.Id,
                GuestName = b.Guest.Name,
                RoomNumber = b.Room.Number,
                CheckIn = b.CheckIn,
                CheckOut = b.CheckOut,
                Status = b.Status
            }).ToList();

            return View("Index", bookings);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Booking not found");
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error managing bookings");
            return View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }

    [HttpPost]
    public IActionResult AddBooking(BookingInputModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid booking details.";
            return RedirectToAction(nameof(Index), new { hotelId = model.HotelId });
        }

        try
        {
            var room = _roomService.GetById(model.RoomId);
            var guest = _guestService.GetById(model.GuestId);

            if (room == null || guest == null)
            {
                TempData["Error"] = "Invalid room or guest selection.";
                return RedirectToAction(nameof(Index), new { hotelId = model.HotelId });
            }

            if (!_bookingService.IsRoomAvailable(model.RoomId, model.CheckIn, model.CheckOut))
            {
                TempData["Error"] = "The selected room is not available for the specified dates.";
                return RedirectToAction(nameof(Index), new { hotelId = model.HotelId });
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                Room = room,
                Guest = guest,
                CheckIn = model.CheckIn,
                CheckOut = model.CheckOut,
                Status = "Confirmed"
            };
            _roomService.UpdateRoomStatus(model.RoomId);

            _bookingService.Create(booking);
            TempData["Success"] = "Booking added successfully!";
            return RedirectToAction(nameof(Index), new { hotelId = model.HotelId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"An error occurred while adding the booking: {ex.Message}";
            return RedirectToAction(nameof(Index), new { hotelId = model.HotelId });
        }
    }

    [HttpPost]
    public IActionResult DeleteBooking(Guid id, Guid hotelId)
    {
        try
        {
            var success = _bookingService.Delete(id);
            if (!success)
            {
                TempData["Error"] = "Failed to delete booking.";
            }
            else
            {
                TempData["Success"] = "Booking deleted successfully!";
            }

            return RedirectToAction(nameof(Index), new { id=hotelId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to delete booking: {ex.Message}";
            return RedirectToAction(nameof(Index), new { hotelId });
        }
    }
}