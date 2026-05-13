using System.Net.Http.Headers;

namespace UberClone.Mobile.Services;

/// <summary>
/// Adds the bearer token (if any) to outgoing API calls.
/// </summary>
public sealed class AuthHandler : DelegatingHandler
{
    private readonly ISessionStore _session;
    public AuthHandler(ISessionStore session) => _session = session;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = await _session.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, ct);
    }
}
