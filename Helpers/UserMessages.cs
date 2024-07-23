using Goddard.Clock.Data;

namespace Goddard.Clock.Helpers;
public class ModalUserMessage(NavigationService? navigation, ClockDatabase? database, string message, bool pauseAndRestartPageTimeouts = false, bool showActivityIndicator = false, int? secondsToShow = null, bool resetNavigationAndGoToMainOnClose = false)
{
    private readonly string _message = message;
    private readonly bool _pauseAndRestartPageTimeouts = pauseAndRestartPageTimeouts;
    private UserMessagePage? _userMessagePage;
    private readonly int? _secondsToShow = secondsToShow;
    private readonly bool _resetNavigationAndGoToMainOnClose = resetNavigationAndGoToMainOnClose;
    private readonly bool _showActivityIndicator = showActivityIndicator;
    private readonly ClockDatabase _database = database ?? throw new ArgumentNullException(nameof(database));
    private readonly NavigationService _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));


    public void Show()
    {
        if (_pauseAndRestartPageTimeouts)
            GlobalResources.Current.GoToMainOnPageTimeout = false;

        _userMessagePage = new UserMessagePage(_message, _showActivityIndicator);

        _ = (Application.Current?.MainPage?.Navigation.PushModalAsync(_userMessagePage, false));

        if (_secondsToShow.HasValue)
        {
            _userMessagePage.Dispatcher.StartTimer(TimeSpan.FromSeconds(_secondsToShow.Value), () =>
            {
                async void PerformAsyncOperation()
                {
                    try
                    {
                        await Close();
                    }
                    catch (Exception ex)
                    {
                        await Logging.Log(_database, ex, "UserMessages.cs StartTimer to Close Exception");
                    }

                }

                PerformAsyncOperation();
                return false;
            });
        }
    }


    public async Task Close()
    {
        try
        {
            if (_userMessagePage != null)
            {
                GlobalResources.Current.UpdateLastUserInteraction();

                if (_pauseAndRestartPageTimeouts)
                    GlobalResources.Current.GoToMainOnPageTimeout = true;

                _navigation.isFromModal = true;
                _ = await _userMessagePage.Navigation.PopModalAsync(false);

                if (_resetNavigationAndGoToMainOnClose)
                  if (_navigation != null) { await _navigation.ResetNavigationAndGoToRoot(); }
            }
        }
        catch (Exception ex)
        {
            await Logging.Log(_database, ex, "UserMessages.cs Close Exception");
        }
    }
}
