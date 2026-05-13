namespace UberClone.Mobile.Models;

public sealed class AppSettings
{
    public string ApiBaseUrl { get; set; } = "https://localhost:50766";
    public string GoogleMapsApiKey { get; set; } = "";

    public static AppSettings Default => new();
}
