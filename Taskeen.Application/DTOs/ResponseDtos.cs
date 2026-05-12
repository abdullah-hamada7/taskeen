using Taskeen.Domain.Enums;

namespace Taskeen.Application.DTOs;

public record UserDto(int Id, string FullName, string IdentityNumber, string Nationality, UserRole Role);

public record TowerDto(int Id, string Name);

public record UnitBedDto(int Id, int BedNumber);

public record UnitDto(
    int Id, 
    string Code, 
    decimal BasePrice, 
    int TotalBeds, 
    TowerDto? Tower, 
    UserDto? Owner, 
    UserDto? Supervisor,
    List<UnitBedDto>? Beds
);

public record BookingDto(
    int Id,
    int UnitId,
    string UnitCode,
    int UserId,
    string UserName,
    DateTime StartDate,
    DateTime EndDate,
    BookingSource Source,
    BookingType Type,
    BookingStatus Status,
    decimal BaseAmount,
    decimal TaxAmount,
    decimal TotalAmount
);

public record FinancialReportDto(
    string Tower,
    string Month,
    decimal TotalRevenue,
    decimal TotalTax,
    decimal TotalBase
);

public record UnitStatusDto(
    string Code,
    int TotalBeds,
    int Occupancy,
    bool IsFull
);

public record SupervisorUnitDto(
    string UnitCode,
    int TotalBeds,
    List<string> Occupants
);
