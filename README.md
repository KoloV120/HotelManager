# HotelManager

HotelManager is an ASP.NET Core MVC application to manage hotels, rooms, guests and bookings. It uses Entity Framework Core with MySQL and ASP.NET Core Identity for authentication. The solution contains the web app, a core library (business logic), a data project (EF Core + repositories) and unit tests.

## Project layout

- [HotelManager.sln](HotelManager.sln) - root solution
- [HotelManager/HotelManager.sln](HotelManager/HotelManager.sln) - web project solution
- Web app: [HotelManager](HotelManager/)
  - Startup / app: [HotelManager/Program.cs](HotelManager/Program.cs)
  - Views: [HotelManager/Views](HotelManager/Views/)
  - Example view: [HotelManager/Views/Guest/Index.cshtml](HotelManager/Views/Guest/Index.cshtml)
  - Layout: [HotelManager/Views/Shared/_Layout.cshtml](HotelManager/Views/Shared/_Layout.cshtml)
- Core logic: [HotelManager.Core](HotelManager.Core/)
  - Services: [`HotelManager.Core.Services.HotelService`](HotelManager.Core/Services/HotelService.cs), [`HotelManager.Core.Services.RoomService`](HotelManager.Core/Services/RoomService.cs), [`HotelManager.Core.Services.BookingService`](HotelManager.Core/Services/BookingService.cs), [`HotelManager.Core.Services.GuestService`](HotelManager.Core/Services/GuestService.cs)
  - Interfaces: [HotelManager.Core/Interfaces](HotelManager.Core/Interfaces/)
- Data layer: [HotelManager.Data](HotelManager.Data/)
  - DbContext: [`HotelManager.Data.HMDbContext`](HotelManager.Data/HMDbContext.cs)
  - Repositories: [`HotelManager.Data.Repositories.IRepository<T>`](HotelManager.Data/Repositories/IRepository.cs), [`HotelManager.Data.Repositories.Repository<TEntity>`](HotelManager.Data/Repositories/Repository.cs)
  - Migrations: [HotelManager.Data/Migrations](HotelManager.Data/Migrations/) (example: [InitialCreate](HotelManager.Data/Migrations/20250427191446_InitialCreate.cs))
  - Design-time factory: [`HotelManager.Data.HotelManDesignTimeFactory`](HotelManager.Data/HotelManDesignTimeFactory.cs)
  - Models / IEntity: [`HotelManager.Data.Models.IIdentifiable`](HotelManager.Data/Models/IIdentifiable.cs)
- Tests: [HotelManager.Tests](HotelManager.Tests/) — unit tests for services/controllers

## Quickstart

Prerequisites:
- .NET SDK (match project target)
- MySQL server (or configure another EF Core provider)
- IDE: Visual Studio / VS Code

1. Configure connection string in:
   - [HotelManager/appsettings.json](HotelManager/appsettings.json) or [HotelManager/appsettings.Development.json](HotelManager/appsettings.Development.json)
2. Build and run:
```sh
dotnet restore
dotnet build
dotnet run --project HotelManager/HotelManager.csproj
```
3. On first run migrations are applied automatically (see [HotelManager/Program.cs](HotelManager/Program.cs) and [HotelManager.Data/Migrations](HotelManager.Data/Migrations/)).

## Database & Migrations

- Migrations are in [HotelManager.Data/Migrations](HotelManager.Data/Migrations/). To add a migration from the solution root:
```sh
dotnet ef migrations add YourName_Migration -s HotelManager/ -p HotelManager.Data/
dotnet ef database update -s HotelManager/ -p HotelManager.Data/
```
Use the design-time factory [`HotelManager.Data.HotelManDesignTimeFactory`](HotelManager.Data/HotelManDesignTimeFactory.cs) if running tooling from the solution root.

## Testing

Run unit tests:
```sh
dotnet test HotelManager.Tests/
```
Tests cover core services (see tests in [HotelManager.Tests/Services](HotelManager.Tests/Services/)).

## Development notes

- Business logic lives in the Core project; wire services via DI in [HotelManager/Program.cs](HotelManager/Program.cs).
- Data access uses repository pattern in [HotelManager.Data/Repositories](HotelManager.Data/Repositories/).
- Projections (DTOs) are under [HotelManager.Core/Projections](HotelManager.Core/Projections/).

Key files:
- [`HotelManager.Core.Services.HotelService`](HotelManager.Core/Services/HotelService.cs)
- [`HotelManager.Core.Services.RoomService`](HotelManager.Core/Services/RoomService.cs)
- [`HotelManager.Data.HMDbContext`](HotelManager.Data/HMDbContext.cs)
- [HotelManager/Program.cs](HotelManager/Program.cs)

## Contributing

- Follow existing code style.
- Add unit tests for new service logic in [HotelManager.Tests](HotelManager.Tests/).

## License

Check repository root for license information.