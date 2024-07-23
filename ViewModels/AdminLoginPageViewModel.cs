using Goddard.Clock.Helpers;
using Goddard.Clock.Factories;

namespace Goddard.Clock.ViewModels;
public class AdminLoginPageViewModel : BaseViewModel
{
    private string? _password;
    private readonly NavigationService _navigation;
    private readonly IServiceProvider _serviceProvider;
    public Command CancelCommand { get; }
    public Command VerifyPasswordCommand { get; }
    public string Password
    {
        get { return _password!; }
        set
        {
            _password = value;
            OnPropertyChanged();
        }
    }


    public AdminLoginPageViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _navigation = App.NavigationService ?? throw new ArgumentNullException(nameof(App.NavigationService));
        VerifyPasswordCommand = new Command(async () =>
        {
            if (String.IsNullOrEmpty(Password))
                _ = (Application.Current?.MainPage?.DisplayAlert("", "password is required", "OK"));
            else if (Password != Settings.Password)
                _ = (Application.Current?.MainPage?.DisplayAlert("", "invalid credentials, please try again", "OK"));
            else
            {
                var factory = _serviceProvider.GetRequiredService<IAdminPageFactory>();
                var page = factory.Create();
                if(Application.Current?.MainPage?.Navigation != null)
                    await Application.Current.MainPage.Navigation.PushAsync(page);
            }
        });
        CancelCommand = new Command(async () =>
        {
          if (_navigation != null) { await _navigation.ResetNavigationAndGoToRoot(); }
        });
    }


}
