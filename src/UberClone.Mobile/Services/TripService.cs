using UberClone.Shared.Dtos;

namespace UberClone.Mobile.Services;

public interface ITripService
{
    Task<IReadOnlyList<TripSummaryDto>> HistoryAsync(UserRole role);
}

public sealed class TripService : ITripService
{
    private readonly IApiClient _api;
    public TripService(IApiClient api) => _api = api;

    public async Task<IReadOnlyList<TripSummaryDto>> HistoryAsync(UserRole role)
    {
        var roleParam = role == UserRole.Driver ? "driver" : "rider";
        var list = await _api.GetAsync<List<TripSummaryDto>>($"/trips?role={roleParam}");
        return list ?? new List<TripSummaryDto>();
    }
}
