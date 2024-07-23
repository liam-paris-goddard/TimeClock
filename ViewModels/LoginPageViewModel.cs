using Goddard.Clock.Data;
using Goddard.Clock.Helpers;
using Goddard.Clock.Factories;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace Goddard.Clock.ViewModels;
public class LoginPageViewModel : BaseViewModel
{
    private readonly TelemetryClient _telemetryClient;

    private string? _loginName;
    private string? _password;
    private readonly ClockDatabase? _database;
    private readonly NavigationService? _navigation;

    private readonly IServiceProvider _serviceProvider;
    public string HeaderText
    {
        get
        {
#if PRODBUILD || PILOTBUILD
            return "Goddard Time & Attendance Clock System";
#elif PILOTQABUILD
            return "Goddard Time & Attendance Clock System QA Pilot";
#else
            return "Goddard Time & Attendance Clock System QA";
#endif
        }
    }

    public LoginPageViewModel(IServiceProvider serviceProvider)
    {
        _telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();
        _serviceProvider = serviceProvider;
        _database = App.Database ?? throw new ArgumentNullException(nameof(App.Database));
        _navigation = App.NavigationService;
        LoginName = Settings.Username;
        Password = Settings.Password;

        LoginCommand = new Command(async () =>
        {
            if (String.IsNullOrEmpty(LoginName) || String.IsNullOrEmpty(Password))
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("", "username and password are required", "OK");
                return;
            }

            Settings.Username = LoginName;
            Settings.Password = Password;

            try
            {
                if (_navigation != null && _database != null)
                {
                    var msg = new ModalUserMessage(_navigation, _database, "Logging in...", true, true);
                    msg.Show();
                    await RetryHelper.RetryOnExceptionAsync(3, TimeSpan.FromSeconds(5), async () =>
    {
        try
        {
            var allowed = await new RestService().GetAllowedSchools(LoginName);
            Settings.IsMultiSchoolUser = false;
            Settings.LastSelectedSchoolID = 0;
            Settings.LastSelectedSchoolName = "";
            if (allowed == null || allowed.Length == 0)
            {
                Settings.Username = "";
                Settings.Password = "";
                LoginName = "";
                Password = "";
                await msg.Close();
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("", "invalid credentials or inadequate permissions, please try again", "OK");

                });
                return;
            }
            if (allowed.Length == 1)
            {
                Settings.LastSelectedSchoolID = allowed.First().ID;
                Settings.LastSelectedSchoolName = allowed?.First().Name ?? "";

                var config = await new RestService().GetSchoolConfiguration(Settings.LastSelectedSchoolID);
                if (config != null)
                {
                    Settings.BypassSignatureEmployees = config.BypassSignatureEmployees;
                    Settings.BypassSignatureParents = config.BypassSignatureParents;
                }
                else
                {
                    Settings.BypassSignatureEmployees = false;
                    Settings.BypassSignatureParents = false;
                }

                await msg.Close();
                if (_navigation != null) { 
                    _telemetryClient.TrackEvent("Login", new Dictionary<string, string> { { "Username", LoginName } });        
                    _telemetryClient.Flush();            
                    await _navigation.ResetNavigationAndGoToRoot(); 
                }
            }
            else
            {
                Settings.IsMultiSchoolUser = true;

                if (allowed.Length <= 10)
                {
                    var factory = _serviceProvider.GetRequiredService<ISchoolSelectionPageFactory>();
                    var schools = allowed.ToList();
                    var page = factory.Create(schools);
                    await msg.Close();
                    _telemetryClient.TrackEvent("Login", new Dictionary<string, string> { { "Username", LoginName } });    
                    _telemetryClient.Flush();                

                    await _navigation.ResetNavigationAndGoToRoot(page);
                }
                else
                {
                    var factory = _serviceProvider.GetRequiredService<IStateSelectionPageFactory>();
                    var page = factory.Create(allowed.ToList());
                    await msg.Close();

                    _telemetryClient.TrackEvent("Login", new Dictionary<string, string> { { "Username", LoginName } });
                    _telemetryClient.Flush();                    
                    await _navigation.ResetNavigationAndGoToRoot(page);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    });

                }
                else
                {
                    throw new Exception("Navigation or Database is null");
                }

            }
            catch
            {
                Settings.Username = "";
                Settings.Password = "";
            }
        });
    }

    public string LoginName
    {
        get { return _loginName!; }
        set
        {
            _loginName = value;
            OnPropertyChanged();
        }
    }

    public string Password
    {
        get { return _password!; }
        set
        {
            _password = value;
            OnPropertyChanged();
        }
    }

    public Command LoginCommand { get; }
}
