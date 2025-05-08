using HotelManager.Data;
using Microsoft.EntityFrameworkCore;
using HotelManager.Core.Configuration;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Set the default culture to en-US
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services to the container.
builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not defined.");
}
builder.Services.AddDbContext<HMDbContext>(options =>
options.UseMySQL(connectionString)); // Use MySQL as the database provider

builder.Services.RegisterServices();    // Register your services here
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<HMDbContext>();
    dbContext.Database.Migrate();  // 👈 this applies migrations automatically
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Add this line to serve static files
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "hotelManagement",
    pattern: "HotelManagement/ManageHotel/{id}",
    defaults: new { controller = "HotelManagement", action = "ManageHotel" });

app.MapControllerRoute(
    name: "roomManagement",
    pattern: "Room/Index/{id}",
    defaults: new { controller = "Room", action = "Index" });
app.MapControllerRoute(
 name: "guestManagement",
 pattern: "Guest/Index/{id}",
 defaults: new { controller = "Guest", action = "Guest" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();