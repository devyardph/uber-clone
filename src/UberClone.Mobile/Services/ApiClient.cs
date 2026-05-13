using System.Net.Http.Json;
using System.Text.Json;

namespace UberClone.Mobile.Services;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string url, CancellationToken ct = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct = default);
    Task PostAsync<TRequest>(string url, TRequest body, CancellationToken ct = default);
    Task DeleteAsync(string url, CancellationToken ct = default);
}

public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public ApiClient(HttpClient http) => _http = http;

    public async Task<T?> GetAsync<T>(string url, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync(url, ct);
        await EnsureSuccess(resp);
        return await resp.Content.ReadFromJsonAsync<T>(Json, ct);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync(url, body, Json, ct);
        await EnsureSuccess(resp);
        return await resp.Content.ReadFromJsonAsync<TResponse>(Json, ct);
    }

    public async Task PostAsync<TRequest>(string url, TRequest body, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync(url, body, Json, ct);
        await EnsureSuccess(resp);
    }

    public async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync(url, ct);
        await EnsureSuccess(resp);
    }

    private static async Task EnsureSuccess(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode) return;
        var body = await resp.Content.ReadAsStringAsync();
        throw new HttpRequestException(
            $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}: {body}",
            inner: null,
            statusCode: resp.StatusCode);
    }
}
