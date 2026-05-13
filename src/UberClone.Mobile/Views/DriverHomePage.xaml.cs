using UberClone.Mobile.ViewModels;

namespace UberClone.Mobile.Views;

public partial class DriverHomePage : ContentPage
{
    private readonly DriverHomeViewModel _vm;

    public DriverHomePage(DriverHomeViewModel vm)
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
