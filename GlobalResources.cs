using System.ComponentModel;

namespace Goddard.Clock;
public class GlobalResources : INotifyPropertyChanged
{
    // Singleton
    public static GlobalResources Current { get; } = new();

    //changing this to 25 (from 30) because 30 just "feels too long" when using app
    public static readonly int PageTimeoutSeconds = 25;

    private bool _goToMainOnPageTimeout;
    public bool GoToMainOnPageTimeout
    {
        get => _goToMainOnPageTimeout;
        set => SetProperty(ref _goToMainOnPageTimeout, value);
    }

    private DateTime _lastUserInteraction;
    public DateTime LastUserInteraction
    {
        get => _lastUserInteraction;
        set => SetProperty(ref _lastUserInteraction, value);
    }

    private DateTime _currentDateTime;
    public DateTime CurrentDateTime
    {
        get => _currentDateTime;
        set => SetProperty(ref _currentDateTime, value);
    }

    public bool HasPageTimedOut => GoToMainOnPageTimeout && LastUserInteraction.AddSeconds(PageTimeoutSeconds) < CurrentDateTime;

    public void UpdateLastUserInteraction() => LastUserInteraction = DateTime.Now;

    public void UpdateCurrentDateTime() => CurrentDateTime = DateTime.Now;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual bool SetProperty<T>(ref T backingStore, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "", Action? onChanged = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        onChanged?.Invoke();
        OnPropertyChanged(propertyName);
        return true;
    }
}