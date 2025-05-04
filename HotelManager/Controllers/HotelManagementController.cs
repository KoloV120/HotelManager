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
    private void SetBookingModalTempData(Guid guestId, string? guestName = null,string? errorMessage = null)
    {
        TempData["Error"] = errorMessage;
        TempData["ShowBookingModal"] = true;
        TempData["GuestId"] = guestId;
        TempData["GuestName"] = guestName ?? _guestService.GetById(guestId)?.Name;
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

    [HttpPost]
    public IActionResult AddGuest([FromForm] GuestInputModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid guest details";
            return RedirectToAction(nameof(ManageHotel), new { id = model.HotelId });
        }

        try
        {
            var guest = new Guest
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone
            };

            _guestService.Create(guest);

            // Store guest info and show booking modal flag
            SetBookingModalTempData(guest.Id, guest.Name);

            return RedirectToAction(nameof(ManageHotel), new { id = model.HotelId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding guest");
            ModelState.AddModelError("", "Error creating guest");
            return View("ManageHotel", _hotelService.GetHotelInfo(model.HotelId));
        }
    }

    [HttpPost]
    public IActionResult AddBooking([FromForm] BookingInputModel model)
    {
        if (!ModelState.IsValid)
        {
            SetBookingModalTempData(model.GuestId,"Invalid booking details");
            return RedirectToAction(nameof(ManageHotel), new { id = model.HotelId });
        }
        var guest = _guestService.GetById(model.GuestId);
        var room = _roomService.GetById(model.RoomId);
        if (guest == null || room == null)
        {
            SetBookingModalTempData(model.GuestId,"Guest or room not found", guest?.Name);
            return RedirectToAction(nameof(ManageHotel), new { id = model.HotelId });
        }

        try
        {
            var booking = new Booking
            {
                CheckIn = model.CheckIn,
                CheckOut = model.CheckOut,
                Status = "confirmed",
                Guest = guest,
                Room = room
            };

            _bookingService.Create(booking);
            TempData["Success"] = "Booking created successfully!";
            TempData.Remove("ShowBookingModal");
            return RedirectToAction(nameof(ManageHotel), new { id = model.HotelId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for guest {GuestId}", model.GuestId);
            SetBookingModalTempData(model.GuestId,"There was an error", guest.Name);
            return RedirectToAction(nameof(ManageHotel), new { id = model.HotelId });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}