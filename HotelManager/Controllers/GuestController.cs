using Microsoft.AspNetCore.Mvc;
using HotelManager.Core.Interfaces;
using HotelManager.Models;
using HotelManager.Data.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;



namespace HotelManager.Controllers;

public class GuestController : Controller
{
    private readonly IGuestService _guestService;
    private readonly IRoomService _roomService;
    private readonly IHotelService _hotelService;
    private readonly ILogger<GuestController> _logger;

    public GuestController(IGuestService guestService, IRoomService roomService, IHotelService hotelService, ILogger<GuestController> logger)
    {
        _guestService = guestService;
        _roomService = roomService;
        _hotelService = hotelService;
        _logger = logger;
    }


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
    public IActionResult CallToReception(Guid id,Guid guestId)
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


