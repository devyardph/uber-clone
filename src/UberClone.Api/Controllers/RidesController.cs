using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using UberClone.Api.Hubs;
using UberClone.Api.Services;
using UberClone.Shared.Dtos;
using UberClone.Shared.Hubs;

namespace UberClone.Api.Controllers;

[ApiController]
[Route("rides")]
[Authorize]
public sealed class RidesController : ControllerBase
{
    private readonly IRideStore _rides;
    private readonly IFareCalculator _fares;
    private readonly IRideMatchingService _matcher;
    private readonly IUserStore _users;
    private readonly IHubContext<RideHub> _hub;

    public RidesController(
        IRideStore rides,
        IFareCalculator fares,
        IRideMatchingService matcher,
        IUserStore users,
        IHubContext<RideHub> hub)
    {
        _rides = rides;
        _fares = fares;
        _matcher = matcher;
        _users = users;
        _hub = hub;
    }

    private string UserId => User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!;

    [HttpPost("estimate")]
    public ActionResult<FareEstimate> Estimate([FromBody] RideRequest req)
        => Ok(_fares.Estimate(req.Pickup, req.Dropoff, req.RideType));

    [HttpPost]
    public async Task<ActionResult<RideDto>> Request([FromBody] RideRequest req)
    {
        var fare = _fares.Estimate(req.Pickup, req.Dropoff, req.RideType);
        var ride = new RideDto(
            Id: Guid.NewGuid().ToString("N"),
            RiderId: UserId,
            DriverId: null,
            Pickup: req.Pickup,
            PickupAddress: req.PickupAddress,
            Dropoff: req.Dropoff,
            DropoffAddress: req.DropoffAddress,
            RideType: req.RideType,
            Status: RideStatus.Requested,
            Fare: fare,
            CreatedAtUtc: DateTime.UtcNow,
            CompletedAtUtc: null,
            CurrentDriverLocation: null);

        _rides.Create(ride);

        // Try to match a driver immediately.
        var match = _matcher.FindNearestDriver(req.Pickup);
        if (match is not null)
        {
            var (driverId, connectionId, _) = match.Value;
            ride = ride with { DriverId = driverId, Status = RideStatus.Assigned };
            _rides.Update(ride);

            await _hub.Clients.Client(connectionId)
                .SendAsync(RideHubEvents.RideAssigned, ride);
        }

        return Ok(ride);
    }

    [HttpGet("{id}")]
    public ActionResult<RideDto> Get(string id)
    {
        var ride = _rides.Get(id);
        return ride is null ? NotFound() : Ok(ride);
    }

    [HttpPost("{id}/status")]
    public async Task<ActionResult<RideDto>> UpdateStatus(string id, [FromBody] RideStatus status)
    {
        var ride = _rides.Get(id);
        if (ride is null) return NotFound();

        var updated = ride with
        {
            Status = status,
            CompletedAtUtc = status == RideStatus.Completed ? DateTime.UtcNow : ride.CompletedAtUtc
        };
        _rides.Update(updated);

        await _hub.Clients.Group($"ride-{id}")
            .SendAsync(RideHubEvents.RideStatusChanged, new RideStatusUpdate(id, status, DateTime.UtcNow));

        if (status == RideStatus.Completed)
            await _hub.Clients.Group($"ride-{id}")
                .SendAsync(RideHubEvents.RideCompleted, updated);

        return Ok(updated);
    }

    [HttpGet("mine")]
    public ActionResult<IEnumerable<RideDto>> Mine([FromQuery] string role = "rider")
        => Ok(role == "driver" ? _rides.ForDriver(UserId) : _rides.ForRider(UserId));
}
