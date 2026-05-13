using System;

namespace UberClone.Mobile.Services;

internal sealed class ActionObserver<T> : IObserver<T>
{
    private readonly Action<T> _onNext;
    private readonly Action<Exception>? _onError;
    private readonly Action? _onCompleted;

    public ActionObserver(Action<T> onNext, Action<Exception>? onError = null, Action? onCompleted = null)
    {
        _onNext = onNext ?? throw new ArgumentNullException(nameof(onNext));
        _onError = onError;
        _onCompleted = onCompleted;
    }

    public void OnCompleted() => _onCompleted?.Invoke();

    public void OnError(Exception error) => _onError?.Invoke(error);

    public void OnNext(T value) => _onNext(value);
}

internal static class ObservableExtensions
{
    public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
        => source.Subscribe(new ActionObserver<T>(onNext));

    public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted)
        => source.Subscribe(new ActionObserver<T>(onNext, onError, onCompleted));
}
