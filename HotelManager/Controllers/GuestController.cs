using Microsoft.AspNetCore.Mvc;
using HotelManager.Core.Interfaces;
using HotelManager.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HotelManager.Controllers;

/// <summary>
/// Controller for managing guests in the hotel management system.
/// </summary>
public class GuestController : Controller
{
    private readonly IGuestService _guestService;
    private readonly IRoomService _roomService;
    private readonly IHotelService _hotelService;
    private readonly ILogger<GuestController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GuestController"/> class.
    /// </summary>
    /// <param name="guestService">The guest service for managing guest-related operations.</param>
    /// <param name="roomService">The room service for managing room-related operations.</param>
    /// <param name="hotelService">The hotel service for managing hotel-related operations.</param>
    /// <param name="logger">The logger for logging information and errors.</param>
    public GuestController(IGuestService guestService, IRoomService roomService, IHotelService hotelService, ILogger<GuestController> logger)
    {
        _guestService = guestService;
        _roomService = roomService;
        _hotelService = hotelService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the list of guests for a specific hotel.
    /// </summary>
    /// <param name="id">The unique identifier of the hotel.</param>
    /// <returns>The view displaying the list of guests.</returns>
    public IActionResult Index(Guid id)
    {
        try
        {
            ViewData["HotelId"] = id;

            var guests = _guestService.GetAllByHotelId(id)
            .Select(g => new GuestViewModel
            {
                Id = g.Id,
                Name = g.Name,
                Email = g.Email,
                Phone = g.Phone,
            }).ToList();

            return View("Index", guests);
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
    /// Calls a guest to the reception.
    /// </summary>
    /// <param name="id">The unique identifier of the hotel.</param>
    /// <param name="guestId">The unique identifier of the guest to call.</param>
    /// <returns>A redirect to the guests index view.</returns>
    public IActionResult CallToReception(Guid id, Guid guestId)
    {
        try
        {
            var guest = _guestService.GetById(guestId);
            if (guest == null)
            {
                TempData["Error"] = "Guest not found.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Message"] = $"Guest {guest.Name} has been called to reception.";
            return RedirectToAction(nameof(Index), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling guest to reception");
            TempData["Error"] = "An error occurred while calling the guest to reception.";
            return RedirectToAction(nameof(Index));
        }
    }
}


