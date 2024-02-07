using TimeClock.Helpers;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace TimeClock.Controls
{
    public class PageHeaderViewModel : BaseViewModel
    {
        public PageHeaderViewModel()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                NetworkIndicator = Colors.White;
            else
                NetworkIndicator = Colors.Red;

            Connectivity.ConnectivityChanged += Current_ConnectivityChanged;
        }

        private void Current_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
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

        public Command GoHomeCommand { get; } = new Command(async () =>
        {
            var alreadyOnHomePage = App.Current.MainPage.Navigation.NavigationStack.Last() is HomePage;

            var isNormalRunMode = App.Current.MainPage.Navigation.NavigationStack.First() is HomePage;

            if (!alreadyOnHomePage && isNormalRunMode)
                await Helpers.Navigation.ResetNavigationAndGoToRoot();
        });

        public Command TapDateCommand { get; } = new Command(async () =>
        {
            if (App.Current.MainPage.Navigation.NavigationStack.Last() is HomePage)
                await App.Current.MainPage.Navigation.PushAsync(new AdminLoginPage());
        });

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
}