using Microsoft.EntityFrameworkCore;
namespace HotelManager.Data;

public class HMDbContext : DbContext
{
    public HMDbContext()
    {

    }
    public HMDbContext(DbContextOptions<HMDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Add any additional configuration here if needed
    }
}

