# Uber Clone - Architecture

A combined Rider/Driver mobile app built with **.NET MAUI on .NET 10** using **MVVM**, backed by an **ASP.NET Core 10 Web API** with **SignalR** for live tracking and **Google Maps** for routing and visualization.

## 1. High-level architecture

```
+----------------------------+         +-------------------------------+
|     UberClone.Mobile       | <-----> |        UberClone.Api          |
|  (.NET MAUI - MVVM)        |  HTTPS  |  (ASP.NET Core 10 + SignalR)  |
|                            |  + WS   |                               |
|  Views (XAML)              |         |  Controllers / Hubs           |
|  ViewModels (CommunityTk)  |         |  Services (matching, fares)   |
|  Services (Http, SignalR,  |         |  In-memory stores (swap for   |
|   Location, Auth, Maps)    |         |   EF Core / Postgres later)   |
|  Custom GoogleMap handler  |         |  JWT issuance + validation    |
+----------------------------+         +-------------------------------+
            |                                   |
            v                                   v
   +-------------------+                +----------------+
   | Google Maps SDK   |                | (future) DB,   |
   | iOS / Android     |                |   Stripe, FCM  |
   +-------------------+                +----------------+
```

## 2. Solution layout

```
UberClone.sln
├── src/
│   ├── UberClone.Shared/      # DTOs + SignalR contracts (referenced by both)
│   ├── UberClone.Api/         # ASP.NET Core 10 Web API + RideHub
│   └── UberClone.Mobile/      # .NET MAUI 10 application
└── docs/ARCHITECTURE.md
```

`UberClone.Shared` is the contract project. Both the API and mobile app reference it, so DTO and message-name drift is prevented at compile time.

## 3. Mobile app - MVVM layering

The mobile app follows a strict three-layer MVVM with dependency injection wired in `MauiProgram.cs`:

**View layer (XAML pages + Shell):** Pure presentation. No code-behind logic except wiring focused UI events. Pages bind to ViewModels via `BindingContext`. Navigation is driven by `Shell.Current.GoToAsync` so routes stay declarative.

**ViewModel layer:** Built on `CommunityToolkit.Mvvm` source generators (`[ObservableProperty]`, `[RelayCommand]`). All ViewModels inherit from `BaseViewModel`, which provides `IsBusy`, `Title`, and an error-handling helper. ViewModels never reference `Microsoft.Maui.*` UI types — they call into services through interfaces only.

**Service layer:** Stateless interfaces (`IAuthService`, `IRideService`, `ILocationService`, `IRideHubClient`, `IApiClient`, `IPaymentService`) implemented in concrete classes. Services own all I/O, persistence (`SecureStorage`), and live updates. The DI container resolves them as singletons or scoped depending on whether they hold connection state.

**Cross-cutting:** A `BaseViewModel.SafeExecuteAsync` wraps commands so exceptions surface as user-friendly toasts/alerts and `IsBusy` is always reset.

## 4. Navigation flow

```
Login ──► RoleSelect ─┬─► RiderShell ──► RiderHome ──► RideTracking ──► TripSummary
                      │                  ├──► TripHistory
                      │                  └──► Payments
                      └─► DriverShell ──► DriverHome ──► DriverActiveTrip ──► EarningsSummary
```

Shell is recreated on role change to avoid mixing navigation stacks.

## 5. Live tracking (SignalR)

A single `RideHub` hosts two groups per ride:

- `ride-{id}` (rider + driver subscribers)
- `driver-region-{cellId}` (drivers in a geo-cell who can be matched)

Server messages: `RideRequested`, `RideAssigned`, `DriverLocationUpdated`, `RideStatusChanged`, `RideCompleted`. The mobile `RideHubClient` exposes these as `IObservable<T>`-style events that ViewModels subscribe to during `OnAppearing` and dispose during `OnDisappearing`.

Driver location is throttled to one update per 3 s on the client and broadcast only to the rider in the active ride to control fan-out.

## 6. Ride matching

`RideMatchingService` (server-side) holds a thread-safe in-memory dictionary of available drivers keyed by a coarse geohash. Matching is a simple "nearest available driver within N km" pass — easy to swap for a queue/dispatcher (Hangfire, Akka.NET) later. The service emits `RideAssigned` to the matched driver's connection.

## 7. Auth

JWT bearer tokens issued by `/auth/login` and `/auth/register`. The mobile `AuthService` stores the token in `SecureStorage` and a typed `HttpClient` adds it via a `DelegatingHandler`. Refresh tokens are scaffolded but not enforced in v1.

## 8. Maps

Google Maps is integrated through a **custom MAUI handler** (`GoogleMapHandler`) that wraps the platform native views (`GMSMapView` on iOS, `MapView` from `Xamarin.GooglePlayServices.Maps` on Android). The cross-platform `GoogleMap` control exposes:

- `Pins` (markers — pickup, dropoff, driver)
- `Polyline` (route)
- `CenterOn(Location, zoom)`
- `AnimateMarker(driverPin, newLocation, duration)` for smooth driver motion

Directions and ETA come from the Google Directions API via `IDirectionsService`.

## 9. Why this design

- **MVVM with CommunityToolkit.Mvvm** keeps ViewModels test-friendly and free of platform code.
- **Shared contracts project** prevents DTO drift across mobile/API.
- **SignalR over polling** gives sub-second tracking updates with managed reconnection.
- **In-memory stores behind interfaces** let the backend run without a database for the demo while leaving a clean seam for EF Core + Postgres.
- **Custom map handler** instead of a third-party MAUI maps library means no version-lock to a community plugin and full access to Google Maps features.

## 10. What's intentionally out of scope for v1

Real payment processing (Stripe/Adyen), surge pricing, driver onboarding/KYC, in-app chat, push notifications (FCM/APNs), background location on iOS (requires entitlements), and persistent storage. Each is documented as an extension point in the relevant service.
