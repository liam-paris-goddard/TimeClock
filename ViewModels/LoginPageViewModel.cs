using TimeClock.Data;
using TimeClock.Helpers;
using TimeClock.Models;
using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Controls;

namespace TimeClock
{
    public class LoginPageViewModel : BaseViewModel
    {
        private string _loginName;
        private string _password;

        public LoginPageViewModel()
        {
            LoginName = Helpers.Settings.Username;
            //Password = Helpers.Settings.Password;

            LoginCommand = new Command(async () =>
            {
                if (LoginName == "x")
                {
                    await Helpers.Navigation.ResetNavigationAndGoToRoot(new TestPage());
                    return;
                }
                else if (String.IsNullOrEmpty(LoginName) || String.IsNullOrEmpty(Password))
                {
                    await App.Current.MainPage.DisplayAlert("", "username and password are required", "OK");
                    return;
                }

                Helpers.Settings.Username = LoginName;
                Helpers.Settings.Password = Password;

                try
                {
                    var msg = new ModalUserMessage("Logging in...", true, true);
                    msg.Show();

                    await RetryHelper.RetryOnExceptionAsync(3, TimeSpan.FromSeconds(5), async () =>
                    {
                        var allowed = await new RestService().GetAllowedSchools(LoginName);

                        Helpers.Settings.IsMultiSchoolUser = false;
                        Helpers.Settings.LastSelectedSchoolID = 0;
                        Helpers.Settings.LastSelectedSchoolName = "";

                        if (allowed == null || allowed.Count() == 0)
                        {
                            Helpers.Settings.Username = "";
                            Helpers.Settings.Password = "";
                            Password = "";
                            msg.Close();
                            await App.Current.MainPage.DisplayAlert("", "invalid credentials or inadequate permissions, please try again", "OK");
                            return;
                        }

                        if (allowed.Count() == 1)
                        {
                            Helpers.Settings.LastSelectedSchoolID = allowed.First().ID;
                            Helpers.Settings.LastSelectedSchoolName = allowed.First().Name;

                            var config = await new RestService().GetSchoolConfiguration(Helpers.Settings.LastSelectedSchoolID);
                            if (config != null)
                            {
                                Helpers.Settings.BypassSignatureEmployees = config.BypassSignatureEmployees;
                                Helpers.Settings.BypassSignatureParents = config.BypassSignatureParents;
                            }
                            else
                            {
                                Helpers.Settings.BypassSignatureEmployees = false;
                                Helpers.Settings.BypassSignatureParents = false;
                            }

                            msg.Close();
                            await Helpers.Navigation.ResetNavigationAndGoToRoot();
                        }
                        else
                        {
                            Helpers.Settings.IsMultiSchoolUser = true;

                            if (allowed.Count() <= 10)
                            {
                                var page = new SchoolSelectionPage();
                                var vm = ((SchoolSelectionPageViewModel)page.BindingContext);
                                vm.Schools = allowed.ToList();

                                msg.Close();
                                await Helpers.Navigation.ResetNavigationAndGoToRoot(page);
                            }
                            else
                            {
                                var page = new StateSelectionPage();
                                var vm = ((StateSelectionPageViewModel)page.BindingContext);
                                vm.Schools = allowed.ToList();

                                msg.Close();
                                await Helpers.Navigation.ResetNavigationAndGoToRoot(page);
                            }
                        }
                    });
                }
                catch
                {
                    Helpers.Settings.Username = "";
                    Helpers.Settings.Password = "";
                }
            });
        }

        public string LoginName
        {
            get { return _loginName; }
            set
            {
                _loginName = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public Command LoginCommand { get; }
    }
}
