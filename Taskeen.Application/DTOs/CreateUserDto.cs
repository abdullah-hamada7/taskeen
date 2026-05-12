using Taskeen.Domain.Enums;

namespace Taskeen.Application.DTOs;

public record CreateUserDto(
    string FullName, 
    string IdentityNumber, 
    string Nationality, 
    UserRole Role, 
    string Password
);
