using CommunityToolkit.Mvvm.ComponentModel;

namespace UberClone.Mobile.ViewModels;

/// <summary>
/// Common state and helpers for all ViewModels. Inherits the source-generated
/// INotifyPropertyChanged plumbing from CommunityToolkit.Mvvm.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string? errorMessage;

    /// <summary>
    /// Wraps async command bodies so exceptions surface as a user-friendly error
    /// and IsBusy is always reset.
    /// </summary>
    protected async Task SafeExecuteAsync(Func<Task> action)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            await action();
        }
        catch (Exception ex)
        {
            ErrorMessage = Friendly(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string Friendly(Exception ex) => ex switch
    {
        HttpRequestException http when http.Message.Contains("401") => "Please sign in again.",
        HttpRequestException http => $"Network error: {http.Message}",
        _ => ex.Message
    };
}
