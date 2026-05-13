namespace UberClone.Shared.Dtos;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string FullName, string PhoneNumber);
public record AuthResponse(string Token, UserDto User);

public record UserDto(string Id, string Email, string FullName, string PhoneNumber, UserRole DefaultRole);

public enum UserRole { Rider, Driver }
