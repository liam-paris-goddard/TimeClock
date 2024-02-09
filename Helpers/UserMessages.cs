using System;
using Microsoft.Maui.Controls;

namespace TimeClock.Helpers
{
    public class ModalUserMessage
    {
        private string _message = "";
        private bool _pauseAndRestartPageTimeouts = false;
        private UserMessagePage _userMessagePage = null;
        private int? _secondsToShow = null;
        private bool _resetNavigationAndGoToMainOnClose = false;
        private bool _showActivityIndicator = false;

        public ModalUserMessage(string message, bool pauseAndRestartPageTimeouts = false, bool showActivityIndicator = false, int? secondsToShow = null, bool resetNavigationAndGoToMainOnClose = false)
        {
            _message = message;
            _pauseAndRestartPageTimeouts = pauseAndRestartPageTimeouts;
            _secondsToShow = secondsToShow;
            _resetNavigationAndGoToMainOnClose = resetNavigationAndGoToMainOnClose;
            _showActivityIndicator = showActivityIndicator;
        }

        public async Task Show()
        {
            if (_pauseAndRestartPageTimeouts)
                GlobalResources.Current.GoToMainOnPageTimeout = false;

            _userMessagePage = new UserMessagePage(_message, _showActivityIndicator);

            await Application.Current.MainPage.Navigation.PushModalAsync(_userMessagePage, false);

            if (_secondsToShow.HasValue)
            {
                _userMessagePage.Dispatcher.DispatchDelayed(async () =>
                {
                    try
                    {
                        await CloseAsync();
                    }
                    catch (Exception ex)
                    {
                        _ = TimeClock.Helpers.Logging.Log(ex, "UserMessages.cs StartTimer to Close Exception");
                        throw;
                    }
                }, TimeSpan.FromSeconds(_secondsToShow.Value));
            }
        }

        public void Close()
        {
            if (_userMessagePage != null)
            {
                GlobalResources.Current.UpdateLastUserInteraction();

                if (_pauseAndRestartPageTimeouts)
                    GlobalResources.Current.GoToMainOnPageTimeout = true;

                _userMessagePage.Navigation.PopModalAsync(false);

                if (_resetNavigationAndGoToMainOnClose)
                    _ = Helpers.Navigation.ResetNavigationAndGoToRoot();
            }
        }
    }
}

/**

 TODO - consider

Also, the Application.Current.MainPage.Navigation.PushModalAsync and PopModalAsync methods are used for modal page navigation in .NET MAUI, just like in Xamarin.Forms. However, .NET MAUI introduces a new handler-based architecture for navigation which you might want to consider using. */