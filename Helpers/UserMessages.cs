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

        public void Show()
        {
            if (_pauseAndRestartPageTimeouts)
                GlobalResources.Current.GoToMainOnPageTimeout = false;

            _userMessagePage = new UserMessagePage(_message, _showActivityIndicator);

            Application.Current.MainPage.Navigation.PushModalAsync(_userMessagePage, false);

            if (_secondsToShow.HasValue)
            {
                Device.StartTimer(TimeSpan.FromSeconds(_secondsToShow.Value), () =>
                {
                    try
                    {
                        Close();
                    }
                    catch (Exception ex)
                    {
                        Goddard.Clock.Helpers.Logging.Log(ex, "UserMessages.cs StartTimer to Close Exception");
                        return false;
                    }
                    return false;
                });
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
                    Helpers.Navigation.ResetNavigationAndGoToRoot();
            }
        }
    }
}

/**

TODO 

The main change here is replacing Xamarin.Forms with Microsoft.Maui.Controls.

Please note that the UserMessagePage and GlobalResources classes, and the Goddard.Clock.Helpers.Logging.Log method, are not defined in the provided code. If these are part of your Xamarin.Forms application, they will also need to be updated for .NET MAUI.

Also, the Application.Current.MainPage.Navigation.PushModalAsync and PopModalAsync methods are used for modal page navigation in .NET MAUI, just like in Xamarin.Forms. However, .NET MAUI introduces a new handler-based architecture for navigation which you might want to consider using. */