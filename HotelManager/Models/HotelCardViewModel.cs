namespace HotelManager.Models;

public class DashboardViewModel
{
    public DashboardViewModel()
    {
        Hotels = new List<HotelCardViewModel>();
    }

    public List<HotelCardViewModel> Hotels { get; set; }
}
public class HotelCardViewModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? City { get; set; }
    public int TotalRooms { get; set; }
}
