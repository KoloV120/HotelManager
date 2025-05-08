using Microsoft.AspNetCore.Mvc;
using HotelManager.Core.Interfaces;
using HotelManager.Models;
using HotelManager.Data.Models;

namespace HotelManager.Controllers;

public class RoomController : Controller
{
    private readonly IRoomService _roomService;
    private readonly IHotelService _hotelService;
    private readonly ILogger<RoomController> _logger;


    public RoomController(IRoomService roomService, IHotelService hotelService,ILogger<RoomController> logger)
    {
        _roomService = roomService;
        _hotelService = hotelService;
        _logger = logger;

    }

    public IActionResult Index(Guid id)
    {
        ViewData["HotelId"] = id; // Pass the HotelId to the view
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