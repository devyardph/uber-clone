using UberClone.Shared.Dtos;

namespace UberClone.Mobile.Services;

public interface IRideService
{
    Task<FareEstimate> EstimateAsync(RideRequest request);
    Task<RideDto> RequestAsync(RideRequest request);
    Task<RideDto?> GetAsync(string rideId);
    Task<RideDto> UpdateStatusAsync(string rideId, RideStatus status);
    Task<IReadOnlyList<RideDto>> MineAsync(UserRole role);
}

public sealed class RideService : IRideService
{
    private readonly IApiClient _api;
    public RideService(IApiClient api) => _api = api;

    public async Task<FareEstimate> EstimateAsync(RideRequest request) =>
        await _api.PostAsync<RideRequest, FareEstimate>("/rides/estimate", request)
        ?? throw new InvalidOperationException("Empty estimate.");

    public async Task<RideDto> RequestAsync(RideRequest request) =>
        await _api.PostAsync<RideRequest, RideDto>("/rides", request)
        ?? throw new InvalidOperationException("Could not create ride.");

    public Task<RideDto?> GetAsync(string rideId) =>
        _api.GetAsync<RideDto>($"/rides/{rideId}");

    public async Task<RideDto> UpdateStatusAsync(string rideId, RideStatus status) =>
        await _api.PostAsync<RideStatus, RideDto>($"/rides/{rideId}/status", status)
        ?? throw new InvalidOperationException("Status update failed.");

    public async Task<IReadOnlyList<RideDto>> MineAsync(UserRole role)
    {
        var roleParam = role == UserRole.Driver ? "driver" : "rider";
        var list = await _api.GetAsync<List<RideDto>>($"/rides/mine?role={roleParam}");
        return list ?? new List<RideDto>();
    }
}
