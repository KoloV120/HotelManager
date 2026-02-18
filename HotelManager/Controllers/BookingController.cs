using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HotelManager.Core.Interfaces;
using HotelManager.Models;
using HotelManager.Data.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace HotelManager.Controllers;

/// <summary>
/// Controller for managing bookings in the hotel management system.
/// </summary>
[Authorize]
public class BookingController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;
    private readonly IGuestService _guestService;
    private readonly IHotelService _hotelService;
    private readonly ILogger<BookingController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookingController"/> class.
    /// </summary>
    /// <param name="bookingService">The booking service.</param>
    /// <param name="roomService">The room service.</param>
    /// <param name="guestService">The guest service.</param>
    /// <param name="hotelService">The hotel service.</param>
    /// <param name="logger">The logger for logging information and errors.</param>
    public BookingController(IBookingService bookingService, IRoomService roomService, IGuestService guestService, IHotelService hotelService, ILogger<BookingController> logger)
    {
        _bookingService = bookingService;
        _roomService = roomService;
        _guestService = guestService;
        _hotelService = hotelService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the list of bookings for a specific hotel.
    /// </summary>
    /// <param name="id">The unique identifier of the hotel.</param>
    /// <returns>The view displaying the list of bookings.</returns>
    public IActionResult Index(Guid id)
    {
        try
        {
            // ensure current user owns the hotel
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var hotel = _hotelService.GetById(id);
            if (hotel == null || hotel.OwnerId != userId)
            {
                _logger.LogWarning("Unauthorized access attempt to bookings for hotel {HotelId} by user {UserId}", id, userId);
                return Forbid();
            }

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

    /// <summary>
    /// Adds a new booking to the system.
    /// </summary>
    /// <param name="model">The booking input model containing booking details.</param>
    /// <returns>A redirect to the bookings index view.</returns>
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

    /// <summary>
    /// Deletes a booking from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the booking to delete.</param>
    /// <param name="hotelId">The unique identifier of the hotel associated with the booking.</param>
    /// <returns>A redirect to the bookings index view.</returns>
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

            return RedirectToAction(nameof(Index), new { id = hotelId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to delete booking: {ex.Message}";
            return RedirectToAction(nameof(Index), new { hotelId });
        }
    }
}