namespace UberClone.Shared.Hubs;

/// <summary>
/// Names of methods invoked on the server by clients.
/// </summary>
public static class RideHubInvoke
{
    public const string JoinRide = nameof(JoinRide);
    public const string LeaveRide = nameof(LeaveRide);
    public const string PublishDriverLocation = nameof(PublishDriverLocation);
    public const string GoOnline = nameof(GoOnline);
    public const string GoOffline = nameof(GoOffline);
}

/// <summary>
/// Names of methods invoked on clients by the server.
/// </summary>
public static class RideHubEvents
{
    public const string RideAssigned = nameof(RideAssigned);
    public const string RideStatusChanged = nameof(RideStatusChanged);
    public const string DriverLocationUpdated = nameof(DriverLocationUpdated);
    public const string RideRequested = nameof(RideRequested);
    public const string RideCompleted = nameof(RideCompleted);
}
