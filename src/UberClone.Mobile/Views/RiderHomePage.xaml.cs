using UberClone.Mobile.ViewModels;

namespace UberClone.Mobile.Views;

public partial class RiderHomePage : ContentPage
{
    private readonly RiderHomeViewModel _vm;

    public RiderHomePage(RiderHomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
