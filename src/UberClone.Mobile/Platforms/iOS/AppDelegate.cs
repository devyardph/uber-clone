using Foundation;

namespace UberClone.Mobile;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    // When wiring the Google Maps iOS SDK, call:
    //   Google.Maps.MapServices.ProvideAPIKey("YOUR_GOOGLE_MAPS_API_KEY");
    // inside FinishedLaunching, before the base call.
}
