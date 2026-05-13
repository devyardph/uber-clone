using UberClone.Shared.Dtos;

namespace UberClone.Mobile.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(string email, string password);
    Task<AuthResponse> RegisterAsync(string email, string password, string fullName, string phone);
    Task LogoutAsync();
}

public sealed class AuthService : IAuthService
{
    private readonly IApiClient _api;
    private readonly ISessionStore _session;

    public AuthService(IApiClient api, ISessionStore session)
    {
        _api = api;
        _session = session;
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        var resp = await _api.PostAsync<LoginRequest, AuthResponse>("/auth/login", new(email, password))
            ?? throw new InvalidOperationException("Empty auth response.");
        await _session.SetSessionAsync(resp.Token, resp.User);
        return resp;
    }

    public async Task<AuthResponse> RegisterAsync(string email, string password, string fullName, string phone)
    {
        var resp = await _api.PostAsync<RegisterRequest, AuthResponse>(
            "/auth/register", new(email, password, fullName, phone))
            ?? throw new InvalidOperationException("Empty auth response.");
        await _session.SetSessionAsync(resp.Token, resp.User);
        return resp;
    }

    public Task LogoutAsync() => _session.ClearAsync();
}
