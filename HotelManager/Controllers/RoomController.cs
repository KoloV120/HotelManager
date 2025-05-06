using Microsoft.AspNetCore.Mvc;
using HotelManager.Core.Interfaces;
using HotelManager.Models;
using HotelManager.Data.Models;

namespace HotelManager.Controllers;

public class RoomController : Controller
{
    private readonly IRoomService _roomService;


    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public IActionResult Index(Guid id)
    {
        ViewData["HotelId"] = id; // Pass the HotelId to the view
        var rooms = _roomService.GetAll();
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
    public IActionResult AddRoom([FromForm] RoomInputModel model)
    {
        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
            Console.WriteLine(error.ErrorMessage); // Log the validation errors
            }
            TempData["Error"] = "Invalid room details.";
            return RedirectToAction(nameof(Index),new { id = model.HotelId });
        }

        try
        {
            var room = new Room
            {
                Id = Guid.NewGuid(),
                Number = model.Number,
                Type = model.Type,
                PricePerNight = model.PricePerNight,
                Status = "available",
                HotelId = model.HotelId
            };

            _roomService.Create(room);
            TempData["Success"] = "Room added successfully!";
            return RedirectToAction(nameof(Index), new { id = model.HotelId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to add room: {ex.Message}";
            return RedirectToAction(nameof(Index), new { id = model.HotelId });
        }
    }

    [HttpPost]
    public IActionResult DeleteRoom(Guid id)
    {
        try
        {
            var success = _roomService.Delete(id);
            if (!success)
            {
                TempData["Error"] = "Failed to delete room.";
            }
            else
            {
                TempData["Success"] = "Room deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to delete room: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}