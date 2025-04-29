using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HotelManager.Models;
using HotelManager.Data;
using HotelManager.Data.Models;

namespace HotelManager.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HMDbContext _context;
    public HomeController(HMDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
    public IActionResult Rooms()
    {
        
        return View();
    }
    public IActionResult RoomsAdd(Room room)
    {
        if (ModelState.IsValid)
        {
            _context.Rooms.Add(room);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return RedirectToAction("Rooms");
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
