using Goddard.Clock.Helpers;
using Goddard.Clock.Factories;
using System.ComponentModel;

namespace Goddard.Clock.ViewModels;
public class PageHeaderViewModel : BaseViewModel
{
    private readonly IServiceProvider _serviceProvider;
    public Command GoHomeCommand { get; }
    public Command TapDateCommand { get; }

    private int _TabletLabelFontSize = 20;
    public int TabletLabelFontSize
    {
        get { return _TabletLabelFontSize; }
        set
        {
            _TabletLabelFontSize = value;
            OnPropertyChanged();
        }
    }

    public LayoutOptions TabletGridLayoutOptions = LayoutOptions.Start;

    private readonly NavigationService _navigation = App.NavigationService ?? throw new ArgumentNullException(nameof(App.NavigationService));
    public PageHeaderViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        GoHomeCommand = new Command(async () =>
        {
            if (Application.Current?.MainPage?.Navigation != null)
            {
                var alreadyOnHomePage = Application.Current.MainPage.Navigation.NavigationStack.Last() is HomePage;

                var isNormalRunMode = Application.Current.MainPage.Navigation.NavigationStack.First() is HomePage;

                if (!alreadyOnHomePage && isNormalRunMode && _navigation != null)
                    if (_navigation != null) { await _navigation.ResetNavigationAndGoToRoot(); }
            }
        });

        TapDateCommand = new Command(async () =>
         {
             if (Application.Current?.MainPage?.Navigation != null)
             {
                 if (Application.Current.MainPage.Navigation.NavigationStack.Last() is HomePage)
                 {
                     var factory = _serviceProvider.GetRequiredService<IAdminLoginPageFactory>();
                     var page = factory.Create();
                     await Application.Current.MainPage.Navigation.PushAsync(page);
                 }
             }
         });
        if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            NetworkIndicator = Colors.White;
        else
            NetworkIndicator = Colors.Red;

        Connectivity.ConnectivityChanged += Current_ConnectivityChanged;
    }

    protected override void OnDeviceInformationChanged(string propertyName)
    {

    }


    private void Current_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess == NetworkAccess.Internet)
        {
            NetworkIndicator = Colors.White;
        }
        else
        {
            NetworkIndicator = Colors.Red;
        }
    }

    private Color _NetworkIndicator = Colors.Black;
    public Color NetworkIndicator
    {
        get { return _NetworkIndicator; }
        set
        {
            _NetworkIndicator = value;
            OnPropertyChanged();
        }
    }
}
