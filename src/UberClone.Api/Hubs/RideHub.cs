using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UberClone.Api.Services;
using UberClone.Shared.Dtos;
using UberClone.Shared.Hubs;

namespace UberClone.Api.Hubs;

[Authorize]
public sealed class RideHub : Hub
{
    private readonly IDriverPresence _presence;
    private readonly IRideStore _rides;

    public RideHub(IDriverPresence presence, IRideStore rides)
    {
        _presence = presence;
        _rides = rides;
    }

    private string UserId => Context.User!.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!;

    public Task JoinRide(string rideId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"ride-{rideId}");

    public Task LeaveRide(string rideId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ride-{rideId}");

    public Task GoOnline(GeoPoint location)
    {
        _presence.GoOnline(UserId, Context.ConnectionId, location);
        return Task.CompletedTask;
    }

    public Task GoOffline()
    {
        _presence.GoOffline(UserId);
        return Task.CompletedTask;
    }

    public async Task PublishDriverLocation(DriverLocationUpdate update)
    {
        _presence.UpdateLocation(UserId, update.Location);

        var ride = _rides.Get(update.RideId);
        if (ride is not null)
        {
            _rides.Update(ride with { CurrentDriverLocation = update.Location });
            await Clients.Group($"ride-{update.RideId}")
                .SendAsync(RideHubEvents.DriverLocationUpdated, update);
        }
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _presence.GoOffline(UserId);
        return base.OnDisconnectedAsync(exception);
    }
}
