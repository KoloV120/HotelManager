using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HotelManager.Models;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Services;

namespace HotelManager.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IBookingService _bookingService;
    private readonly IGuestService _guestService;
    private readonly IRoomService _roomService;

    public HomeController(
        IBookingService bookingService,
        IGuestService guestService,
        IRoomService roomService,
        ILogger<HomeController> logger)
    {
        _bookingService = bookingService;
        _guestService = guestService;
        _roomService = roomService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var allGuests = _guestService.GetAll();
            var allRooms = _roomService.GetAll();
            var allBookings = _bookingService.GetAll();

            var viewModel = new DashboardViewModel
            {
                TotalGuests = allGuests.Count(),
                AvailableRooms = allRooms.Count(r => r.Status == "available"),
                ActiveBookings = allBookings.Count(b => b.Status == "active"),
                MonthlyRevenue = allBookings
                    .Where(b => b.CheckIn.Month == DateTime.Now.Month)
                    .Sum(b => b.Room.PricePerNight * (b.CheckOut - b.CheckIn).Days),
                RecentBookings = allBookings
                    .OrderByDescending(b => b.CheckIn)
                    .Take(5)
                    .Select(b => new RecentBookingInfo
                    {
                        GuestName = b.Guest.Name,
                        RoomNumber = b.Room.Number.ToString(),
                        CheckIn = b.CheckIn,
                        CheckOut = b.CheckOut
                    })
                    .ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
