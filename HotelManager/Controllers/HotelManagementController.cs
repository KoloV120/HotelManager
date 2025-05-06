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
    private void SetBookingModalTempData(Guid guestId, string? errorMessage = null, string? guestName = null)
    {
        TempData["Error2"] = errorMessage;
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
    public IActionResult AddRoom(RoomInputModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid room details.";
            return RedirectToAction("ManageHotel", new { id = model.HotelId });
        }

        var hotel = _hotelService.GetById(model.HotelId);
        if (hotel == null)
        {
            TempData["Error"] = "Invalid hotel selection.";
            return RedirectToAction(nameof(ManageHotel), new { id = model.HotelId });
        }

        try
        {
            int RoomsPerFloor = _hotelService.GetRoomsPerFloor(model.HotelId);
            int RoomsInHotel = _hotelService.GetAllRooms(model.HotelId).Count();
            _logger.LogDebug("RoomsInHotel for HotelId {HotelId}: {RoomsInHotel}", model.HotelId, RoomsInHotel);
            int NumberOfRoom = 100 + RoomsInHotel / RoomsPerFloor * 100 + (RoomsInHotel % RoomsPerFloor) + 1;

            var room = new Room
            {
                Id = Guid.NewGuid(),
                HotelId = model.HotelId,
                Number = NumberOfRoom,
                Type = model.Type,
                PricePerNight = model.PricePerNight,
                Status = "Available"
            };
            
            _roomService.Create(room); // Assuming you have a service to handle room operations
            hotel.Rooms.Add(room);
            return RedirectToAction("ManageHotel", new { id = model.HotelId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding room to hotel with ID {HotelId}", model.HotelId);
            TempData["Error"] = "An error occurred while adding the room.";
            return RedirectToAction("ManageHotel", new { id = model.HotelId });
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
            SetBookingModalTempData(guest.Id, null, guest.Name);

            return RedirectToAction(nameof(ManageHotel), new { id = model.HotelId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Failed to create guest: " + ex.Message;
            return View("ManageHotel", _hotelService.GetHotelInfo(model.HotelId));
        }
    }

    [HttpPost]
    public IActionResult AddBooking([FromForm] BookingInputModel model)
    {
        if (!ModelState.IsValid)
        {
            SetBookingModalTempData(model.GuestId, "Invalid booking details");
            return RedirectToAction(nameof(ManageHotel), new { id = model.HotelId });
        }
        var guest = _guestService.GetById(model.GuestId);
        var room = _roomService.GetById(model.RoomId);
        if (guest == null || room == null)
        {
            TempData["Error"] = $"could not find guest or room with the provided IDs.";
            TempData.Remove("ShowBookingModal");
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
            if (!_bookingService.IsRoomAvailable(room.Id, booking.CheckOut, booking.CheckIn))
            {
                SetBookingModalTempData(model.GuestId, $"Room {room.Number} is already booked for the selected dates.");
                return RedirectToAction(nameof(ManageHotel), new { id = model.HotelId });
            }
            _bookingService.Create(booking);
            TempData["Success"] = "Booking created successfully!";
            TempData.Remove("ShowBookingModal");
            return RedirectToAction(nameof(ManageHotel), new { id = model.HotelId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to create booking: {ex.Message}";
            TempData.Remove("ShowBookingModal");
            return RedirectToAction(nameof(ManageHotel), new { id = model.HotelId });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}