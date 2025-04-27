using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
namespace HotelManager.Data;

public class HotelManDesignTimeFactory : IDesignTimeDbContextFactory<HMDbContext>
{
   public HMDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../HotelManager");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<HMDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not found or is empty.");
        }

        optionsBuilder.UseMySQL(connectionString);
        Console.WriteLine("Provider: " + optionsBuilder.Options.Extensions.FirstOrDefault()?.GetType().Name);
        return new HMDbContext(optionsBuilder.Options);
    }
}
