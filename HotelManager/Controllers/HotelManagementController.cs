using Microsoft.AspNetCore.Mvc;
using HotelManager.Core.Interfaces;
using HotelManager.Models;
using System.Diagnostics;
using HotelManager.Data.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
            var dashboardData = _hotelService.GetHotelInfo(id);
            var guests = _guestService.GetAllMinified();

            var viewModel = new HotelManagerViewModel
            {
                TotalGuests = dashboardData.TotalGuests,
                AvailableRooms = dashboardData.AvailableRooms.Count(),
                ActiveBookings = dashboardData.ActiveBookings.Count(),
                MonthlyRevenue = dashboardData.MonthlyRevenue,
                RecentBookings = dashboardData.RecentBookings.Select(b => new RecentBookingInfo
                {
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    GuestName = b.GuestName,
                    RoomNumber = b.RoomNumber
                }).ToList(),
                ListOfAvailableRooms = dashboardData.AvailableRooms.Select(r => new RoomSelectListItem
                {
                    Id = r.Id,
                    Number = r.Number,
                    Type = r.Type,
                    PricePerNight = r.PricePerNight
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