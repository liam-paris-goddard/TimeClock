using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TimeClock.Helpers
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/* TODO

This code should be placed in a file named BaseViewModel.cs inside a ViewModels folder in your .NET MAUI project.

*/