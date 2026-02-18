using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HotelManager.Core.Interfaces;
using HotelManager.Models;
using HotelManager.Data.Models;

namespace HotelManager.Controllers;

/// <summary>
/// Controller for managing rooms in the hotel management system.
/// </summary>
[Authorize]
public class RoomController : Controller
{
    private readonly IRoomService _roomService;
    private readonly IHotelService _hotelService;
    private readonly ILogger<RoomController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoomController"/> class.
    /// </summary>
    /// <param name="roomService">The room service for managing room-related operations.</param>
    /// <param name="hotelService">The hotel service for managing hotel-related operations.</param>
    /// <param name="logger">The logger for logging information and errors.</param>
    public RoomController(IRoomService roomService, IHotelService hotelService, ILogger<RoomController> logger)
    {
        _roomService = roomService;
        _hotelService = hotelService;
        _logger = logger;

    }

    /// <summary>
    /// Displays the list of rooms for a specific hotel.
    /// </summary>
    /// <param name="id">The unique identifier of the hotel.</param>
    /// <returns>The view displaying the list of rooms.</returns>
    public IActionResult Index(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var hotel = _hotelService.GetById(id);
        if (hotel == null || hotel.OwnerId != userId)
        {
            _logger.LogWarning("Unauthorized access attempt to rooms for hotel {HotelId} by user {UserId}", id, userId);
            return Forbid();
        }

        ViewData["HotelId"] = id;
        var rooms = _roomService.GetAllByHotelId(id);
        var viewModel = rooms.Select(r => new RoomViewModel
        {
            Id = r.Id,
            Number = r.Number,
            Type = r.Type,
            PricePerNight = r.PricePerNight,
            Status = r.Status
        }).ToList();

        return View(viewModel);
    }

    /// <summary>
    /// Adds a new room to the specified hotel.
    /// </summary>
    /// <param name="model">The input model containing room details.</param>
    /// <returns>A redirect to the rooms index view.</returns>
    [HttpPost]
    public IActionResult AddRoom(RoomInputModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid room details.";
            return RedirectToAction(nameof(Index), new { id = model.HotelId });
        }

        var hotel = _hotelService.GetById(model.HotelId);
        if (hotel == null)
        {
            TempData["Error"] = "Invalid hotel selection.";
            return RedirectToAction(nameof(Index), new { id = model.HotelId });
        }

        try
        {
            int roomsPerFloor = _hotelService.GetRoomsPerFloor(model.HotelId);
            int roomsInHotel = _hotelService.GetAllRooms(model.HotelId).Count();
            int roomNumber = 100 + roomsInHotel / roomsPerFloor * 100 + (roomsInHotel % roomsPerFloor) + 1;

            var room = new Room
            {
                Id = Guid.NewGuid(),
                HotelId = model.HotelId,
                Number = roomNumber,
                Type = model.Type,
                PricePerNight = model.PricePerNight,
                Status = "Available"
            };

            _roomService.Create(room);
            TempData["Success"] = "Room added successfully!";
            return RedirectToAction(nameof(Index), new { id = model.HotelId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"An error occurred while adding the room: {ex.Message}";
            return RedirectToAction(nameof(Index), new { id = model.HotelId });
        }
    }

    /// <summary>
    /// Edits the details of an existing room.
    /// </summary>
    /// <param name="model">The input model containing updated room details.</param>
    /// <returns>A redirect to the rooms index view.</returns>
    [HttpPost]
    public IActionResult EditRoom(RoomInputModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid room details.";
            return RedirectToAction(nameof(Index), new { id = model.HotelId });
        }

        try
        {
            var room = _roomService.GetById(model.Id);
            if (room == null)
            {
                TempData["Error"] = "Room not found.";
                return RedirectToAction(nameof(Index), new { id = model.HotelId });
            }

            room.Number = model.Number;
            room.Type = model.Type;
            room.PricePerNight = model.PricePerNight;

            _roomService.Update(room);
            TempData["Success"] = "Room updated successfully!";
            return RedirectToAction(nameof(Index), new { id = room.HotelId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to update room: {ex.Message}";
            return RedirectToAction(nameof(Index), new { id = model.HotelId });
        }
    }

    /// <summary>
    /// Deletes a room from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the room to delete.</param>
    /// <returns>A redirect to the rooms index view.</returns>
    [HttpPost]
    public IActionResult DeleteRoom(Guid id)
    {
        try
        {
            var hotelId = _roomService.GetById(id)?.HotelId;
            var success = _roomService.Delete(id);
            if (!success)
            {
                TempData["Error"] = "Failed to delete room.";
            }
            else
            {
                TempData["Success"] = "Room deleted successfully!";
            }

            return RedirectToAction(nameof(Index), new { id = hotelId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to delete room: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}