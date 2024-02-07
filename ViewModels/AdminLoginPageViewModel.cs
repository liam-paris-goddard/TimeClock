using TimeClock.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace TimeClock
{
    public class AdminLoginPageViewModel : BaseViewModel
    {
        private string _password;

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public AdminLoginPageViewModel()
        {
            VerifyPasswordCommand = new Command(() =>
            {
                if (String.IsNullOrEmpty(Password))
                    App.Current.MainPage.DisplayAlert("", "password is required", "OK");
                else if (Password != Helpers.Settings.Password)
                    App.Current.MainPage.DisplayAlert("", "invalid credentials, please try again", "OK");
                else
                    App.Current.MainPage.Navigation.PushAsync(new AdminPage());
            });
        }

        public Command CancelCommand { get; } = new Command(async () =>
        {
            await Helpers.Navigation.ResetNavigationAndGoToRoot();
        });

        public Command VerifyPasswordCommand { get; } = new Command(() =>
        {

        });
    }
}
