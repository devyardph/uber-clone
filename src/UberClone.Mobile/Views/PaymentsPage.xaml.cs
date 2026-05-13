using UberClone.Mobile.ViewModels;

namespace UberClone.Mobile.Views;

public partial class PaymentsPage : ContentPage
{
    private readonly PaymentsViewModel _vm;

    public PaymentsPage(PaymentsViewModel vm)
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
