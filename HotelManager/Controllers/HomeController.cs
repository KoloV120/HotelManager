using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HotelManager.Models;
using HotelManager.Core.Interfaces;
using HotelManager.Data.Models;

namespace HotelManager.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHotelService _hotelService;

    public HomeController(
        IHotelService hotelService,
        ILogger<HomeController> logger)
    {
        _hotelService = hotelService;
        _logger = logger;
    }

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
                Email = model.Email
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
