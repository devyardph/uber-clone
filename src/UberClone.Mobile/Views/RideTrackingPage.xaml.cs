using UberClone.Mobile.ViewModels;

namespace UberClone.Mobile.Views;

public partial class RideTrackingPage : ContentPage
{
    private readonly RideTrackingViewModel _vm;

    public RideTrackingPage(RideTrackingViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.AppearingCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Dispose();
    }
}
