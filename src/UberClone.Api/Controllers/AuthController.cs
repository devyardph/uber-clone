using Microsoft.AspNetCore.Mvc;
using UberClone.Api.Services;
using UberClone.Shared.Dtos;

namespace UberClone.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IUserStore _users;
    private readonly ITokenService _tokens;

    public AuthController(IUserStore users, ITokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    [HttpPost("login")]
    public ActionResult<AuthResponse> Login([FromBody] LoginRequest req)
    {
        var user = _users.FindByEmail(req.Email);
        if (user is null || user.PasswordHash != InMemoryUserStore.Hash(req.Password))
            return Unauthorized(new { message = "Invalid email or password" });

        return Ok(Build(user));
    }

    [HttpPost("register")]
    public ActionResult<AuthResponse> Register([FromBody] RegisterRequest req)
    {
        if (_users.FindByEmail(req.Email) is not null)
            return Conflict(new { message = "Email already registered" });

        var user = _users.Create(req.Email, req.Password, req.FullName, req.PhoneNumber, UserRole.Rider);
        return Ok(Build(user));
    }

    private AuthResponse Build(StoredUser user)
    {
        var token = _tokens.Issue(user);
        var dto = new UserDto(user.Id, user.Email, user.FullName, user.PhoneNumber, user.DefaultRole);
        return new AuthResponse(token, dto);
    }
}
