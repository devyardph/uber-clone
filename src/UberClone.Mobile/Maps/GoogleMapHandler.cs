using Microsoft.Maui.Handlers;

namespace UberClone.Mobile.Maps;

/// <summary>
/// Cross-platform handler shell. The actual native view is created in the
/// platform-specific partial files (Platforms/Android/.. and Platforms/iOS/..).
/// </summary>
#if ANDROID
public partial class GoogleMapHandler : Microsoft.Maui.Handlers.ViewHandler<GoogleMap, global::Android.Gms.Maps.MapView>
#elif IOS
public partial class GoogleMapHandler : Microsoft.Maui.Handlers.ViewHandler<GoogleMap, UIKit.UIView>
#else
public partial class GoogleMapHandler : Microsoft.Maui.Handlers.ViewHandler<GoogleMap, object>
#endif
{
    public static readonly IPropertyMapper<GoogleMap, GoogleMapHandler> Mapper =
        new PropertyMapper<GoogleMap, GoogleMapHandler>(Microsoft.Maui.Handlers.ViewHandler.ViewMapper)
    {
        [nameof(GoogleMap.Center)] = MapCenter,
        [nameof(GoogleMap.Zoom)] = MapZoom,
        [nameof(GoogleMap.Pins)] = MapPins,
        [nameof(GoogleMap.Polyline)] = MapPolyline,
    };

    public GoogleMapHandler() : base(Mapper) { }

    static partial void MapCenter(GoogleMapHandler handler, GoogleMap view);
    static partial void MapZoom(GoogleMapHandler handler, GoogleMap view);
    static partial void MapPins(GoogleMapHandler handler, GoogleMap view);
    static partial void MapPolyline(GoogleMapHandler handler, GoogleMap view);
}
