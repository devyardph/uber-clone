using UberClone.Shared.Dtos;

namespace UberClone.Mobile.Services;

public interface IPaymentService
{
    Task<IReadOnlyList<PaymentMethodDto>> ListAsync();
    Task<PaymentMethodDto> AddAsync(string brand, string last4);
    Task RemoveAsync(string id);
}

public sealed class PaymentService : IPaymentService
{
    private readonly IApiClient _api;
    public PaymentService(IApiClient api) => _api = api;

    public async Task<IReadOnlyList<PaymentMethodDto>> ListAsync() =>
        (await _api.GetAsync<List<PaymentMethodDto>>("/payments/methods")) ?? new();

    public async Task<PaymentMethodDto> AddAsync(string brand, string last4) =>
        await _api.PostAsync<AddPaymentMethodRequest, PaymentMethodDto>(
            "/payments/methods", new(brand, last4))
        ?? throw new InvalidOperationException("Failed to add payment method.");

    public Task RemoveAsync(string id) => _api.DeleteAsync($"/payments/methods/{id}");
}
