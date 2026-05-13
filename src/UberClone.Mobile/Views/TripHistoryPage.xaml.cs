using UberClone.Mobile.ViewModels;

namespace UberClone.Mobile.Views;

public partial class TripHistoryPage : ContentPage
{
    private readonly TripHistoryViewModel _vm;

    public TripHistoryPage(TripHistoryViewModel vm)
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
