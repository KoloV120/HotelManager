using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace HotelManager.Data;
using HotelManager.Data.Models;

// Extend IdentityDbContext so ASP.NET Identity tables are stored in the same DB
public class HMDbContext : IdentityDbContext<IdentityUser>
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

        // keep any custom model configuration here
    }
}
