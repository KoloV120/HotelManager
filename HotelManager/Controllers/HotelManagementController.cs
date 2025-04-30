using Microsoft.AspNetCore.Mvc;
using HotelManager.Core.Interfaces;
using HotelManager.Models;
using System.Diagnostics;

namespace HotelManager.Controllers;

public class HotelManagementController : Controller
{
    private readonly ILogger<HotelManagementController> _logger;
    private readonly IHotelService _hotelService;
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;
    private readonly IGuestService _guestService;

    public HotelManagementController(
        IHotelService hotelService,
        IBookingService bookingService,
        IRoomService roomService,
        IGuestService guestService,
        ILogger<HotelManagementController> logger)
    {
        _hotelService = hotelService;
        _bookingService = bookingService;
        _roomService = roomService;
        _guestService = guestService;
        _logger = logger;
    }

    public IActionResult ManageHotel(Guid id)
    {
        try
        {
            var hotel = _hotelService.GetById(id);
            if (hotel == null)
            {
                return NotFound();
            }

            var bookings = _bookingService.GetAll()
                .Where(b => b.Room.HotelId == id);

            var viewModel = new HotelManagerViewModel
            {
                TotalGuests = bookings.Count(b => b.CheckIn <= DateTime.Now && b.CheckOut >= DateTime.Now),
                AvailableRooms = _roomService.GetAll().Count(r => r.HotelId == id && !r.Bookings.Any(b => b.CheckOut >= DateTime.Now)),
                ActiveBookings = bookings.Count(b => b.CheckIn <= DateTime.Now && b.CheckOut >= DateTime.Now),
                MonthlyRevenue = bookings
                    .Where(b => b.CheckIn.Month == DateTime.Now.Month)
                    .Sum(b => b.Room.PricePerNight * (b.CheckOut - b.CheckIn).Days),
                RecentBookings = bookings
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

            ViewData["HotelName"] = hotel.Name;
            return View("ManageHotel", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error managing hotel {HotelId}", id);
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}