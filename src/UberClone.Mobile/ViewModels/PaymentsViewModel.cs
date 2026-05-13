using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UberClone.Mobile.Services;
using UberClone.Shared.Dtos;

namespace UberClone.Mobile.ViewModels;

public partial class PaymentsViewModel : BaseViewModel
{
    private readonly IPaymentService _payments;

    public ObservableCollection<PaymentMethodDto> Methods { get; } = new();

    [ObservableProperty] private string newBrand = "Visa";
    [ObservableProperty] private string newLast4 = "";

    public PaymentsViewModel(IPaymentService payments)
    {
        _payments = payments;
        Title = "Payment methods";
    }

    [RelayCommand]
    public Task LoadAsync() => SafeExecuteAsync(async () =>
    {
        Methods.Clear();
        foreach (var m in await _payments.ListAsync()) Methods.Add(m);
    });

    [RelayCommand]
    private Task AddAsync() => SafeExecuteAsync(async () =>
    {
        if (NewLast4.Length < 2) return;
        var added = await _payments.AddAsync(NewBrand, NewLast4);
        Methods.Add(added);
        NewLast4 = "";
    });

    [RelayCommand]
    private Task RemoveAsync(PaymentMethodDto method) => SafeExecuteAsync(async () =>
    {
        await _payments.RemoveAsync(method.Id);
        Methods.Remove(method);
    });
}
