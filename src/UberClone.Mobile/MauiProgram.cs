using System.Text.Json;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using UberClone.Mobile.Maps;
using UberClone.Mobile.Models;
using UberClone.Mobile.Services;
using UberClone.Mobile.ViewModels;
using UberClone.Mobile.Views;

namespace UberClone.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<GoogleMap, GoogleMapHandler>();
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Settings.
        builder.Services.AddSingleton(_ => LoadSettings());

        // Session.
        builder.Services.AddSingleton<ISessionStore, SecureSessionStore>();

        // Http stack.
        builder.Services.AddTransient<AuthHandler>();
        builder.Services.AddHttpClient<IApiClient, ApiClient>((sp, http) =>
        {
            var settings = sp.GetRequiredService<AppSettings>();
            var baseUrl = settings.ApiBaseUrl;

            // When running on the Android emulator, "localhost" must be mapped to
            // the host machine using 10.0.2.2. Replace automatically for developer
            // convenience so the app can connect to a locally-run API.
#if ANDROID
            if (baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase) || baseUrl.Contains("127.0.0.1"))
            {
                baseUrl = baseUrl.Replace("localhost", "10.0.2.2", StringComparison.OrdinalIgnoreCase)
                                 .Replace("127.0.0.1", "10.0.2.2", StringComparison.OrdinalIgnoreCase);
            }
#endif

            http.BaseAddress = new Uri(baseUrl);
        })
        // In development accept the local dev certificate so HTTPS to localhost works
        // from emulator/simulator. This is unsafe for production and only enabled
        // in DEBUG builds.
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
#if DEBUG
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
#else
            return new HttpClientHandler();
#endif
        })
        .AddHttpMessageHandler<AuthHandler>();

        // Domain services.
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IRideService, RideService>();
        builder.Services.AddSingleton<ITripService, TripService>();
        builder.Services.AddSingleton<IPaymentService, PaymentService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();
        builder.Services.AddSingleton<IRideHubClient, RideHubClient>();

        // ViewModels.
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<RoleSelectViewModel>();
        builder.Services.AddTransient<RiderHomeViewModel>();
        builder.Services.AddTransient<RideTrackingViewModel>();
        builder.Services.AddTransient<TripHistoryViewModel>();
        builder.Services.AddTransient<PaymentsViewModel>();
        builder.Services.AddTransient<DriverHomeViewModel>();
        builder.Services.AddTransient<DriverActiveTripViewModel>();

        // Pages.
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<RoleSelectPage>();
        builder.Services.AddTransient<RiderHomePage>();
        builder.Services.AddTransient<RideTrackingPage>();
        builder.Services.AddTransient<TripHistoryPage>();
        builder.Services.AddTransient<PaymentsPage>();
        builder.Services.AddTransient<DriverHomePage>();
        builder.Services.AddTransient<DriverActiveTripPage>();

        return builder.Build();
    }

    private static AppSettings LoadSettings()
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<AppSettings>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? AppSettings.Default;
        }
        catch
        {
            return AppSettings.Default;
        }
    }
}
