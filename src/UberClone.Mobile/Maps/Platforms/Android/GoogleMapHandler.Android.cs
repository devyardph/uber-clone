#if ANDROID
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;

namespace UberClone.Mobile.Maps;

public partial class GoogleMapHandler
{
    private global::Android.Gms.Maps.GoogleMap? _native;
    private global::Android.Gms.Maps.Model.Marker? _driverMarker;

    protected override global::Android.Gms.Maps.MapView CreatePlatformView()
    {
        MapsInitializer.Initialize(Context);
        var mv = new global::Android.Gms.Maps.MapView(Context!);
        mv.OnCreate(Bundle.Empty);
        mv.OnResume();
        mv.GetMapAsync(new MapReadyCallback(this));
        return mv;
    }

    // Called by the MapReadyCallback (avoids making the handler implement Android Java interfaces)
    private void OnMapReady(global::Android.Gms.Maps.GoogleMap googleMap)
    {
        _native = googleMap;
        _native.UiSettings.MyLocationButtonEnabled = true;
        ApplyAll();
    }

    private void ApplyAll()
    {
        if (_native is null || VirtualView is null) return;
        ApplyCenter(VirtualView);
        ApplyPins(VirtualView);
        ApplyPolyline(VirtualView);
    }

    static partial void MapCenter(GoogleMapHandler handler, GoogleMap view) => handler.ApplyCenter(view);
    static partial void MapZoom(GoogleMapHandler handler, GoogleMap view) => handler.ApplyCenter(view);
    static partial void MapPins(GoogleMapHandler handler, GoogleMap view) => handler.ApplyPins(view);
    static partial void MapPolyline(GoogleMapHandler handler, GoogleMap view) => handler.ApplyPolyline(view);

    private void ApplyCenter(GoogleMap view)
    {
        if (_native is null || view.Center is null) return;
        var pos = new LatLng(view.Center.Latitude, view.Center.Longitude);
        _native.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(pos, (float)view.Zoom));
    }

    private void ApplyPins(GoogleMap view)
    {
        if (_native is null) return;
        _native.Clear();
        _driverMarker = null;
        foreach (var pin in view.Pins)
        {
            var opts = new MarkerOptions()
                .SetPosition(new LatLng(pin.Location.Latitude, pin.Location.Longitude))
                .SetTitle(pin.Label ?? pin.Kind.ToString());

            // Color-code by kind.
            var color = pin.Kind switch
            {
                MapPinKind.Pickup => BitmapDescriptorFactory.HueGreen,
                MapPinKind.Dropoff => BitmapDescriptorFactory.HueRed,
                _ => BitmapDescriptorFactory.HueAzure
            };
            opts.SetIcon(BitmapDescriptorFactory.DefaultMarker(color));

            var marker = _native.AddMarker(opts);
            if (pin.Kind == MapPinKind.Driver) _driverMarker = marker;
        }
    }

    private void ApplyPolyline(GoogleMap view)
    {
        if (_native is null || view.Polyline is null) return;
        var opts = new PolylineOptions().InvokeWidth(8);
        foreach (var p in view.Polyline)
            opts.Add(new LatLng(p.Latitude, p.Longitude));
        _native.AddPolyline(opts);
    }

    // Helper callback class to bridge Android map readiness back to the handler
    private class MapReadyCallback : Java.Lang.Object, IOnMapReadyCallback
    {
        private readonly GoogleMapHandler _handler;
        public MapReadyCallback(GoogleMapHandler handler) => _handler = handler;
        public void OnMapReady(global::Android.Gms.Maps.GoogleMap googleMap) => _handler.OnMapReady(googleMap);
    }
}
#endif
