using Microsoft.EntityFrameworkCore;
using Taskeen.Domain.Entities;
using Taskeen.Domain.Enums;
using System.Text.Json;
using System.IO;

namespace Taskeen.Infrastructure.Persistence;

public static class TaskeenDbInitializer
{
    public static async Task SeedAsync(TaskeenDbContext context)
    {
        var seedFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Persistence", "seed_data.json");
        
        // Fallback for different build environments (e.g. dotnet run vs bin)
        if (!File.Exists(seedFilePath))
            seedFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Taskeen.Infrastructure", "Persistence", "seed_data.json");

        if (!File.Exists(seedFilePath)) return;

        var json = await File.ReadAllTextAsync(seedFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<SeedData>(json, options);

        if (data == null) return;

        // 1. Towers
        if (!await context.Towers.AnyAsync())
        {
            foreach (var tower in data.Towers)
            {
                tower.CreatedBy = "System";
                tower.CreatedAt = DateTime.UtcNow;
            }
            await context.Towers.AddRangeAsync(data.Towers);
            await context.SaveChangesAsync();
        }

        // 2. Users
        if (!await context.Users.AnyAsync())
        {
            foreach (var userSeed in data.Users)
            {
                var user = new User
                {
                    FullName = userSeed.FullName,
                    IdentityNumber = userSeed.IdentityNumber,
                    Nationality = userSeed.Nationality,
                    Role = userSeed.Role,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userSeed.Password),
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                };
                await context.Users.AddAsync(user);
            }
            await context.SaveChangesAsync();
        }

        // 3. Units
        if (!await context.Units.AnyAsync())
        {
            foreach (var unitSeed in data.Units)
            {
                var tower = await context.Towers.FirstAsync(t => t.Name == unitSeed.TowerName);
                var owner = await context.Users.FirstAsync(u => u.IdentityNumber == unitSeed.OwnerIdentity);
                var supervisor = await context.Users.FirstAsync(u => u.IdentityNumber == unitSeed.SupervisorIdentity);

                var unit = new Unit
                {
                    Code = unitSeed.Code,
                    TowerId = tower.Id,
                    OwnerId = owner.Id,
                    SupervisorId = supervisor.Id,
                    BasePrice = unitSeed.BasePrice,
                    TotalBeds = unitSeed.TotalBeds,
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                };
                await context.Units.AddAsync(unit);
            }
            await context.SaveChangesAsync();
        }

        // 4. Beds (Always ensure they exist for any unit)
        var existingUnits = await context.Units.Include(u => u.Beds).ToListAsync();
        foreach (var unit in existingUnits)
        {
            if (unit.Beds == null || !unit.Beds.Any())
            {
                for (int i = 1; i <= unit.TotalBeds; i++)
                {
                    context.UnitBeds.Add(new UnitBed { UnitId = unit.Id, BedNumber = i, CreatedBy = "System", CreatedAt = DateTime.UtcNow });
                }
            }
        }
        await context.SaveChangesAsync();

        // 5. Sample Bookings (Only if none exist)
        if (!await context.Bookings.AnyAsync())
        {
            var unitA = await context.Units.FirstAsync(u => u.Code == "A-101");
            var guest = await context.Users.FirstAsync(u => u.IdentityNumber == "GST001");

            var bookings = new List<Booking>
            {
                new Booking 
                { 
                    UnitId = unitA.Id, 
                    UserId = guest.Id, 
                    StartDate = DateTime.UtcNow.AddDays(1), 
                    EndDate = DateTime.UtcNow.AddDays(5), 
                    Source = BookingSource.Website, 
                    Type = BookingType.GuestBooking, 
                    Status = BookingStatus.Confirmed,
                    BaseAmount = unitA.BasePrice * 4,
                    TaxAmount = (unitA.BasePrice * 4) * 0.20m,
                    TotalAmount = (unitA.BasePrice * 4) * 1.20m,
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                }
            };
            await context.Bookings.AddRangeAsync(bookings);
            await context.SaveChangesAsync();
        }
    }

    private class SeedData
    {
        public List<Tower> Towers { get; set; } = new();
        public List<UserSeedDto> Users { get; set; } = new();
        public List<UnitSeedDto> Units { get; set; } = new();
    }

    private class UserSeedDto
    {
        public string FullName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public Taskeen.Domain.Enums.UserRole Role { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    private class UnitSeedDto
    {
        public string Code { get; set; } = string.Empty;
        public string TowerName { get; set; } = string.Empty;
        public string OwnerIdentity { get; set; } = string.Empty;
        public string SupervisorIdentity { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int TotalBeds { get; set; }
    }
}
