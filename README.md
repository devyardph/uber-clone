# Uber Clone (.NET MAUI 10 + ASP.NET Core 10)

Combined Rider/Driver app + backend API. MVVM throughout the mobile app, SignalR for live tracking, Google Maps via a custom MAUI handler.

See [ARCHITECTURE.md](./ARCHITECTURE.md) for the design.

## Prerequisites

- .NET 10 SDK
- Workloads: `dotnet workload install maui`
- Visual Studio 2026 / Rider 2026 (with MAUI workload)
- A **Google Maps API key** with Maps SDK for Android, Maps SDK for iOS, and Directions API enabled

## Configure the Google Maps key

1. Put your key in `src/UberClone.Mobile/Resources/Raw/appsettings.json`:
   ```json
   { "GoogleMapsApiKey": "YOUR_KEY", "ApiBaseUrl": "https://localhost:5001" }
   ```
2. Android: replace `YOUR_GOOGLE_MAPS_API_KEY` in `Platforms/Android/AndroidManifest.xml`.
3. iOS: replace the placeholder in `Platforms/iOS/AppDelegate.cs`.

## Run

```bash
# Backend
cd src/UberClone.Api
dotnet run

# Mobile (in another terminal)
cd src/UberClone.Mobile
dotnet build -t:Run -f net10.0-android   # or net10.0-ios
```

## Demo accounts

The API seeds two users on startup:

- Rider: `rider@demo.com` / `Password1!`
- Driver: `driver@demo.com` / `Password1!`

Choose a role on the Role Select screen after login.

## Project layout

```
UberClone.sln
└── src/
    ├── UberClone.Shared/   # DTOs, hub method names
    ├── UberClone.Api/      # ASP.NET Core 10 + SignalR
    └── UberClone.Mobile/   # .NET MAUI 10 (MVVM)
```
