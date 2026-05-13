using UberClone.Mobile.Views;

namespace UberClone.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("role-select", typeof(RoleSelectPage));
        Routing.RegisterRoute("rider-home", typeof(RiderHomePage));
        Routing.RegisterRoute("ride-tracking", typeof(RideTrackingPage));
        Routing.RegisterRoute("trip-history", typeof(TripHistoryPage));
        Routing.RegisterRoute("payments", typeof(PaymentsPage));
        Routing.RegisterRoute("driver-home", typeof(DriverHomePage));
        Routing.RegisterRoute("driver-active-trip", typeof(DriverActiveTripPage));
    }
}
