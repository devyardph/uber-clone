using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UberClone.Api.Services;
using UberClone.Shared.Dtos;

namespace UberClone.Api.Controllers;

[ApiController]
[Route("trips")]
[Authorize]
public sealed class TripsController : ControllerBase
{
    private readonly IRideStore _rides;
    private readonly IUserStore _users;

    public TripsController(IRideStore rides, IUserStore users)
    {
        _rides = rides;
        _users = users;
    }

    private string UserId => User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!;

    [HttpGet]
    public ActionResult<IEnumerable<TripSummaryDto>> History([FromQuery] string role = "rider")
    {
        var rides = (role == "driver" ? _rides.ForDriver(UserId) : _rides.ForRider(UserId))
            .Where(r => r.Status == RideStatus.Completed)
            .OrderByDescending(r => r.CompletedAtUtc);

        var summaries = rides.Select(r =>
        {
            var driverName = r.DriverId is null ? "Unassigned" : (_users.FindById(r.DriverId)?.FullName ?? "Driver");
            var duration = r.CompletedAtUtc.HasValue
                ? (int)(r.CompletedAtUtc.Value - r.CreatedAtUtc).TotalMinutes
                : 0;
            return new TripSummaryDto(
                r.Id,
                r.PickupAddress,
                r.DropoffAddress,
                r.Fare.Amount,
                r.Fare.Currency,
                r.CompletedAtUtc ?? DateTime.UtcNow,
                driverName,
                r.Fare.DistanceKm,
                duration);
        });

        return Ok(summaries);
    }
}
