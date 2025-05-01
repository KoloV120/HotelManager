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
            var dashboardData = _hotelService.GetHotelDashboard(id);
            var guests = _guestService.GetAllMinified();
            var rooms = _roomService.GetAllMinified()
                .Where(r => r.HotelId == id && r.Status != "available");

            var viewModel = new HotelManagerViewModel
            {
                TotalGuests = dashboardData.TotalGuests,
                AvailableRooms = dashboardData.AvailableRooms,
                ActiveBookings = dashboardData.ActiveBookings,
                MonthlyRevenue = dashboardData.MonthlyRevenue,
                RecentBookings = dashboardData.RecentBookings.Select(b => new RecentBookingInfo
                {
                    GuestName = b.Guest.Name,
                    RoomNumber = b.Room.Number,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                }).ToList(),
                AvailableGuests = guests.Select(g => new GuestSelectListItem
                {
                    Id = g.Id,
                    Name = g.Name
                }).ToList(),
                ListOfAvailableRooms = rooms.Select(r => new RoomSelectListItem
                {
                    Id = r.Id,
                    Number = r.Number,
                    PricePerNight = r.PricePerNight,
                    Type = r.Type
                }).ToList()
            };

            ViewData["HotelName"] = dashboardData.HotelName;
            return View("ManageHotel", viewModel);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Hotel not found");
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error managing hotel {HotelId}", id);
            return View("Error", new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}