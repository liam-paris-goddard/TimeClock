using Goddard.Clock.Data;
using Goddard.Clock.Helpers;
using Goddard.Clock.ViewModels;

namespace Goddard.Clock;
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AdminPage : TimedContentPage
{
    private readonly ClockDatabase _database;
    private readonly SyncEngineService _syncEngine = App.SyncEngine ?? throw new ArgumentNullException(nameof(App.SyncEngine));
    private readonly NavigationService _navigation = App.NavigationService ?? throw new ArgumentNullException(nameof(App.NavigationService));
    public AdminPage(AdminPageViewModel viewModel)
    {
        _database = App.Database ?? throw new ArgumentNullException(nameof(App.Database));
        InitializeComponent();
        BindingContext = viewModel;
        createButtonGrid(DeviceOrientation, DeviceWidth);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
                    await ((AdminPageViewModel)BindingContext).Init();

        listviewEvents.ItemsSource = MyBinding?.PendingClockEvents;

    }
    protected AdminPageViewModel? MyBinding => BindingContext as AdminPageViewModel;

    private void createButtonGrid(DisplayOrientation orientation, double width)
    {
        adminCommandsGrid.ColumnDefinitions.Clear();
        adminCommandsGrid.RowDefinitions.Clear();
        if (orientation.ToString() == "Portrait" && width <= 1500)
        {
            adminCommandsGrid.HeightRequest = 150;
            adminCommandsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            adminCommandsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            adminCommandsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            adminCommandsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            // Set the buttons to the appropriate row and column for small screens
            Grid.SetRow(logoutButton, 0);
            Grid.SetColumn(logoutButton, 0);
            Grid.SetRow(rebuildButton, 0);
            Grid.SetColumn(rebuildButton, 1);
            Grid.SetRow(forceSyncButton, 1);
            Grid.SetColumn(forceSyncButton, 0);
            Grid.SetRow(changeSchoolButton, 1);
            Grid.SetColumn(changeSchoolButton, 1);
        }
        else
        {
            adminCommandsGrid.HeightRequest = 90;
            adminCommandsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            adminCommandsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            adminCommandsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            adminCommandsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            adminCommandsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            // Set the buttons to the appropriate row and column for larger screens
            Grid.SetRow(logoutButton, 0);
            Grid.SetColumn(logoutButton, 0);
            Grid.SetRow(rebuildButton, 0);
            Grid.SetColumn(rebuildButton, 1);
            Grid.SetRow(forceSyncButton, 0);
            Grid.SetColumn(forceSyncButton, 2);
            Grid.SetRow(changeSchoolButton, 0);
            Grid.SetColumn(changeSchoolButton, 3);
        }


    }
    protected override void OnDeviceInformationChanged(string propertyName)
    {
        base.OnDeviceInformationChanged(propertyName);
        createButtonGrid(DeviceOrientation, DeviceWidth);
    }




    private async void forceSync_Clicked(object? sender, EventArgs e)
    {
        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            await DisplayAlert("", "no network/internet connection available, resolve your connection before attempting force sync", "OK");
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var msg = new ModalUserMessage(_navigation, _database, "Synchronizing database...", true, true);
                    msg.Show();

                    await _syncEngine.Start();
                    _ = await _syncEngine.ForceSend();
                    _ = await _syncEngine.ForcePull();

                    if(MyBinding != null)
                        await MyBinding.Init();

                    if(MyBinding != null)
                        listviewEvents.ItemsSource = MyBinding.PendingClockEvents;

                    await msg.Close();
                }
                catch (Exception ex)
                {
                    await Logging.Log(_database, ex);
                    throw;
                }
            });
        }
    }
}
