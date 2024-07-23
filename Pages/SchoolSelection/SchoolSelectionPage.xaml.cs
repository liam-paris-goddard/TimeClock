using Goddard.Clock.Controls;
using Goddard.Clock.Data;
using Goddard.Clock.Helpers;
using Goddard.Clock.ViewModels;
using Microsoft.ApplicationInsights;

namespace Goddard.Clock;
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SchoolSelectionPage : TimedContentPage
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ClockDatabase _database;
    private readonly NavigationService _navigation = App.NavigationService ?? throw new ArgumentNullException(nameof(App.NavigationService));
    private readonly SyncEngineService _syncEngine = App.SyncEngine ?? throw new ArgumentNullException(nameof(App.SyncEngine));
    public SchoolSelectionPage(SchoolSelectionPageViewModel viewModel, TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
        _database = App.Database ?? throw new ArgumentNullException(nameof(App.Database));
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected async void PagedGoddardButtonGridButtonClick(object? sender, PagedGoddardButtonGrid.ButtonClickEventArgs e)
    {
        var modalUserMessage = new ModalUserMessage(_navigation, _database, "Synchronizing database...", true, true);
        modalUserMessage.Show();

        if (long.TryParse(e.SelectedValue, out long selectedSchoolID))
        {
            Settings.LastSelectedSchoolID = selectedSchoolID;
        }
        else
        {
            // Handle the case when e.SelectedValue is null or cannot be parsed as a long.
            // You can choose to set a default value or handle the error accordingly.
            // For example:
            Settings.LastSelectedSchoolID = 0;
        }

        Settings.LastSelectedSchoolName = e.SelectedText ?? "";
        _telemetryClient.TrackEvent("SchoolSelected", new Dictionary<string, string> { { "SchoolID", Settings.LastSelectedSchoolID.ToString() }, { "SchoolName", Settings.LastSelectedSchoolName }});
        _telemetryClient.Flush();
        await _syncEngine.Stop();

        var configTask = new RestService().GetSchoolConfiguration(Settings.LastSelectedSchoolID);
        var startSyncTask = _syncEngine.Start();

        await Task.WhenAll(new List<Task>() { configTask, startSyncTask });

        var config = await configTask;

        if (configTask.Result != null)
        {
            Settings.BypassSignatureEmployees = configTask.Result.BypassSignatureEmployees;
            Settings.BypassSignatureParents = configTask.Result.BypassSignatureParents;
        }
        else
        {
            Settings.BypassSignatureEmployees = false;
            Settings.BypassSignatureParents = false;
        }

        _ = modalUserMessage.Close();

      if (_navigation != null) { await _navigation.ResetNavigationAndGoToRoot(); }
    }
}
