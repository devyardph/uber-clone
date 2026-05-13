using UberClone.Mobile.ViewModels;

namespace UberClone.Mobile.Views;

public partial class RoleSelectPage : ContentPage
{
    public RoleSelectPage(RoleSelectViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
