using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HotelManager.Models;
using HotelManager.Core.Interfaces;
using HotelManager.Data.Models;

namespace HotelManager.Controllers;

/// <summary>
/// Controller for managing the home page and hotel-related operations.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHotelService _hotelService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    /// <param name="hotelService">The hotel service for managing hotel-related operations.</param>
    /// <param name="logger">The logger for logging information and errors.</param>
    public HomeController(
        IHotelService hotelService,
        ILogger<HomeController> logger)
    {
        _hotelService = hotelService;
        _logger = logger;
    }

    /// <summary>
    /// Displays the dashboard with a list of hotels.
    /// </summary>
    /// <returns>The view displaying the dashboard.</returns>
    public IActionResult Index()
    {
        try
        {
            var hotels = _hotelService.GetAll();

            var viewModel = new DashboardViewModel
            {
                Hotels = hotels.Select(h => new HotelCardViewModel
                {
                    Id = h.Id,
                    Name = h.Name,
                    City = h.City,
                    TotalRooms = h.Rooms.Count
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hotels");
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    /// <summary>
    /// Adds a new hotel to the system.
    /// </summary>
    /// <param name="model">The input model containing hotel details.</param>
    /// <returns>A redirect to the dashboard view.</returns>
    public IActionResult AddHotel([FromForm] HotelInputModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var hotel = new Hotel
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                City = model.City,
                Address = model.Address,
                Email = model.Email,
                RoomsPerFloor = model.RoomsPerFloor
            };

            bool success = _hotelService.Create(hotel);
            if (!success)
            {
                ModelState.AddModelError("", "Failed to create hotel");
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding hotel");
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    /// <summary>
    /// Deletes a hotel from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the hotel to delete.</param>
    /// <returns>A redirect to the dashboard view.</returns>
    [HttpPost]
    public IActionResult DeleteHotel([FromForm] Guid id)
    {
        try
        {
            bool success = _hotelService.Delete(id);

            if (!success)
            {
                ModelState.AddModelError("", "Failed to delete hotel");
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting hotel with ID {HotelId}", id);
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    /// <summary>
    /// Displays the error page.
    /// </summary>
    /// <returns>The error view with error details.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
