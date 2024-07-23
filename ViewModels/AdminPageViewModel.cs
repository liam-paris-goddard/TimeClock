using System.Collections.ObjectModel;
using Goddard.Clock.Data;
using Goddard.Clock.Factories;
using Goddard.Clock.Helpers;
using Goddard.Clock.Models;
namespace Goddard.Clock.ViewModels;
public class AdminPageViewModel : BaseViewModel
{

    private readonly ClockDatabase _database;
    private readonly NavigationService _navigation;
    private readonly SyncEngineService _syncEngine;

    private DateTime? _latestPullDateTime;

    public DateTime? LatestPullDateTime
    {
        get { return _latestPullDateTime; }
        set
        {
            _latestPullDateTime = value;
            OnPropertyChanged();
        }
    }

    private DateTime? _latestSendDateTime;

    public DateTime? LatestSendDateTime
    {
        get { return _latestSendDateTime; }
        set
        {
            _latestSendDateTime = value;
            OnPropertyChanged();
        }
    }

    private bool _isMultiSchoolUser = true;
    public bool IsMultiSchoolUser
    {
        get { return _isMultiSchoolUser; }
        set
        {
            _isMultiSchoolUser = value;
            OnPropertyChanged();
        }
    }

    private IServiceProvider _serviceProvider;

    private ObservableCollection<EventExtended> _pendingClockEvents = new ObservableCollection<EventExtended>();
    public ObservableCollection<EventExtended> PendingClockEvents
    {
        get { return _pendingClockEvents; }
        set
        {
            _pendingClockEvents = value;
            OnPropertyChanged();
        }
    }

    public AllowedSchool[]? AllowedSchools { get; private set; }

    public class StateItem
    {
        public string? Display { get; set; }
        public string? Value { get; set; }
    }

    public class SchoolItem
    {
        public string? Display { get; set; }
        public long Value { get; set; }
    }

    public AdminPageViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _database = App.Database ?? throw new ArgumentNullException(nameof(App.Database));
;
        _navigation = App.NavigationService ?? throw new ArgumentNullException(nameof(App.NavigationService));
        _syncEngine = App.SyncEngine ?? throw new ArgumentNullException(nameof(App.SyncEngine));
        _ = Init();
        LogoutCommand = new Command(async () =>
        {
            GlobalResources.Current.GoToMainOnPageTimeout = false;

            Settings.Username = "";
            Settings.Password = "";
            if(_syncEngine != null)
                await _syncEngine.Stop();
            var factory = _serviceProvider.GetRequiredService<ILoginPageFactory>();
            var page = factory.Create();
            if(_navigation != null)
                await _navigation.ResetNavigationAndGoToRoot(page);
        });


        RebuildCommand = new Command(async () =>
        {
            try
            {
                var msg = new ModalUserMessage(_navigation, _database, "Rebuilding database...", true, true);
                msg.Show();
                if(_syncEngine != null) {
                    await _syncEngine.Stop();
                    _database.RebuildDatatabase();
                    await _syncEngine.Start();
                }

                await msg.Close();

                await Init();
            }
            catch (Exception ex)
            {
                await Logging.Log(_database, ex);
                throw;
            }
        });

        ChangeSchoolCommand = new Command(async () =>
        {
            GlobalResources.Current.GoToMainOnPageTimeout = false;

            var allowed = await new RestService().GetAllowedSchools(Settings.Username);
            if (allowed == null || allowed.Length <= 1)
                return;
            else
            {
                if (allowed.Length <= 10)
                {
                    var factory = _serviceProvider.GetRequiredService<ISchoolSelectionPageFactory>();
                    var schools = allowed.ToList();
                    var page = factory.Create(schools);
                    if (Application.Current?.MainPage?.Navigation != null)
                        await Application.Current.MainPage.Navigation.PushAsync(page);
                }
                else
                {
                    var factory = _serviceProvider.GetRequiredService<IStateSelectionPageFactory>();
                    var page = factory.Create(allowed.ToList());
                    if (Application.Current?.MainPage?.Navigation != null)
                        await Application.Current.MainPage.Navigation.PushAsync(page);
                }
            }
        });

    }

    public async Task Init()
    {
        try {
        IsMultiSchoolUser = Settings.IsMultiSchoolUser;
        LatestPullDateTime = await _database.GetLocalSyncLogLatest(LogType.Pull);
        LatestSendDateTime = await _database.GetLocalSyncLogLatest(LogType.Send);
        var events = await _database.GetUnprocessedClockExtendedEvents(Int32.MaxValue);
        PendingClockEvents.Clear();
        foreach (var item in events)
            PendingClockEvents.Add(item);
        } catch (Exception ex) {
            await Logging.Log(_database, ex);
            throw;
        }
    }

    public Command LogoutCommand { get; }
    public Command RebuildCommand { get; }
    public Command ChangeSchoolCommand { get; }
}
//public Command ForceSyncCommand { get; }}
