#if IOS
using CoreLocation;
using UIKit;

namespace UberClone.Mobile.Maps;

/// <summary>
/// iOS placeholder. Wire up Google.Maps.iOS bindings once added to the project
/// (see https://developers.google.com/maps/documentation/ios-sdk).
/// This stub keeps the build green; replace with GMSMapView + GMSCameraPosition
/// + GMSMarker calls mirroring GoogleMapHandler.Android.cs.
/// </summary>
public partial class GoogleMapHandler : Microsoft.Maui.Handlers.ViewHandler<GoogleMap, UIView>
{
    protected override UIView CreatePlatformView() => new UIView { BackgroundColor = UIColor.SystemBackground };

    static partial void MapCenter(GoogleMapHandler handler, GoogleMap view) { /* TODO: GMSCameraPosition */ }
    static partial void MapZoom(GoogleMapHandler handler, GoogleMap view) { /* TODO */ }
    static partial void MapPins(GoogleMapHandler handler, GoogleMap view) { /* TODO: GMSMarker */ }
    static partial void MapPolyline(GoogleMapHandler handler, GoogleMap view) { /* TODO: GMSPolyline */ }
}
#endif
