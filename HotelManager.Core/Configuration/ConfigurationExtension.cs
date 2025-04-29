using System;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Services;
using HotelManager.Data.Models;
using HotelManager.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManager.Core.Configuration;

public static class ConfigurationExtensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        // Register Repositories
        services.AddScoped<IRepository<Hotel>, Repository<Hotel>>();
        services.AddScoped<IRepository<Room>, Repository<Room>>();
        services.AddScoped<IRepository<Guest>, Repository<Guest>>();
        services.AddScoped<IRepository<Booking>, Repository<Booking>>();

        // Register Services
        services.AddScoped<IHotelService, HotelService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IGuestService, GuestService>();
        services.AddScoped<IBookingService, BookingService>();
    }
}