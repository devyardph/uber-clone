using System.Collections.ObjectModel;
using UberClone.Shared.Dtos;

namespace UberClone.Mobile.Maps;

/// <summary>
/// Cross-platform Google Map control. The platform-specific GoogleMapHandler
/// turns this into a real Google Map view on each target.
/// </summary>
public sealed class GoogleMap : View
{
    public static readonly BindableProperty CenterProperty = BindableProperty.Create(
        nameof(Center), typeof(GeoPoint), typeof(GoogleMap), default(GeoPoint));

    public static readonly BindableProperty ZoomProperty = BindableProperty.Create(
        nameof(Zoom), typeof(double), typeof(GoogleMap), 13.0);

    public static readonly BindableProperty PinsProperty = BindableProperty.Create(
        nameof(Pins), typeof(ObservableCollection<MapPin>), typeof(GoogleMap),
        defaultValueCreator: _ => new ObservableCollection<MapPin>());

    public static readonly BindableProperty PolylineProperty = BindableProperty.Create(
        nameof(Polyline), typeof(IList<GeoPoint>), typeof(GoogleMap), default(IList<GeoPoint>));

    public GeoPoint? Center
    {
        get => (GeoPoint?)GetValue(CenterProperty);
        set => SetValue(CenterProperty, value);
    }

    public double Zoom
    {
        get => (double)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    public ObservableCollection<MapPin> Pins
    {
        get => (ObservableCollection<MapPin>)GetValue(PinsProperty);
        set => SetValue(PinsProperty, value);
    }

    public IList<GeoPoint>? Polyline
    {
        get => (IList<GeoPoint>?)GetValue(PolylineProperty);
        set => SetValue(PolylineProperty, value);
    }
}

public enum MapPinKind { Pickup, Dropoff, Driver }

public sealed class MapPin
{
    public required GeoPoint Location { get; init; }
    public string? Label { get; init; }
    public MapPinKind Kind { get; init; }
}
