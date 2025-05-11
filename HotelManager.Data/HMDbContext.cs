using Microsoft.EntityFrameworkCore;
namespace HotelManager.Data;
using HotelManager.Data.Models;

public class HMDbContext : DbContext
{
    public HMDbContext()
    {
    }

    public HMDbContext(DbContextOptions<HMDbContext> options) : base(options)
    {
    }
    
    public DbSet<Hotel> Hotels { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Guest> Guests { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
