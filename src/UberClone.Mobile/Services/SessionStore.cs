using UberClone.Shared.Dtos;

namespace UberClone.Mobile.Services;

public interface ISessionStore
{
    Task<string?> GetTokenAsync();
    Task SetSessionAsync(string token, UserDto user);
    Task ClearAsync();
    UserDto? CurrentUser { get; }
    UserRole ActiveRole { get; set; }
}

public sealed class SecureSessionStore : ISessionStore
{
    private const string TokenKey = "uber_clone_token";
    private const string UserIdKey = "uber_clone_user_id";
    private const string UserNameKey = "uber_clone_user_name";
    private const string UserEmailKey = "uber_clone_user_email";

    public UserDto? CurrentUser { get; private set; }
    public UserRole ActiveRole { get; set; } = UserRole.Rider;

    public async Task<string?> GetTokenAsync()
    {
        var token = await SecureStorage.Default.GetAsync(TokenKey);
        if (token is not null && CurrentUser is null)
        {
            var id = await SecureStorage.Default.GetAsync(UserIdKey) ?? "";
            var name = await SecureStorage.Default.GetAsync(UserNameKey) ?? "";
            var email = await SecureStorage.Default.GetAsync(UserEmailKey) ?? "";
            CurrentUser = new UserDto(id, email, name, "", UserRole.Rider);
        }
        return token;
    }

    public async Task SetSessionAsync(string token, UserDto user)
    {
        CurrentUser = user;
        await SecureStorage.Default.SetAsync(TokenKey, token);
        await SecureStorage.Default.SetAsync(UserIdKey, user.Id);
        await SecureStorage.Default.SetAsync(UserNameKey, user.FullName);
        await SecureStorage.Default.SetAsync(UserEmailKey, user.Email);
    }

    public Task ClearAsync()
    {
        CurrentUser = null;
        SecureStorage.Default.RemoveAll();
        return Task.CompletedTask;
    }
}
