using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Maui.Controls;
using TimeClock.Models;
using TimeClock.Data;
using TimeClock.Helpers;

namespace TimeClock
{
    public partial class PinPadPage : TimedContentPage
    {
        private const int MAX_ATTEMPTS = 3;
        private const int MIN_DIGITS = 4;
        private const int MAX_DIGITS = 8;

        private UserType _userType;
        private long? _userPersonId = null;
        private string _userFirstName = "";
        private string _userLastName = "";
        private int _attemptCount = 0;
        private bool _setPINMode = false;
        private string _originalPIN = "";
        private string _newPINFirstEntry = "";
        private string _parentNameStart = "";

        public PinPadPage(UserType userType, string personName, long? personId = null, bool setPINMode = false, string originalPIN = "", bool showPinResetMessageOnLoad = false)
        {
            InitializeComponent();

            _userType = userType;
            _userPersonId = personId;
            _setPINMode = setPINMode;
            _originalPIN = originalPIN;

            if (_userType == UserType.Employee)
            {
                if (!_userPersonId.HasValue)
                {
                    Helpers.Logging.Log("Error - Pin Pad - null user ID");
                    DisplayAlert("Error", "an error has occurred while attempting to load the pin pad", "OK");
                    Helpers.Navigation.ResetNavigationAndGoToRoot();
                }
            }
            else
            {
                _parentNameStart = personName;
            }

            footer.CenterText = personName;

            if (showPinResetMessageOnLoad)
            {
                DisplayAlert("", "your pin has been reset, you are required to change it, enter your new pin", "OK");
            }
        }

        private void Footer_RightButtonClickHandler(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (_setPINMode)
                    {
                        if (!_userPersonId.HasValue)
                        {
                            Helpers.Logging.Log("Error - Pin Pad - Null user ID");
                            await DisplayAlert("Error", "an error has occurred", "OK");
                            return;
                        }

                        if (String.IsNullOrWhiteSpace(_newPINFirstEntry))
                        {
                            if (String.IsNullOrWhiteSpace(pinDisplay.Text) || pinDisplay.Text.Length < MIN_DIGITS || pinDisplay.Text.Length > MAX_DIGITS)
                            {
                                await DisplayAlert("", $"your new pin must be between {MIN_DIGITS} and {MAX_DIGITS} digits long", "OK");
                                pinDisplay.Text = "";
                                return;
                            }

                            if (_userType == UserType.Parent)
                            {
                                var isValid = await App.Database.CheckUniquenessPIN(_userPersonId.Value, _parentNameStart, pinDisplay.Text);
                                if (!isValid)
                                {
                                    await DisplayAlert("", "the specified pin is not availaible, please choose another", "OK");
                                    pinDisplay.Text = "";
                                    return;
                                }
                            }

                            _newPINFirstEntry = pinDisplay.Text;
                            pinDisplay.Text = "";
                            await DisplayAlert("", "confirm your new pin", "OK");
                            return;
                        }
                        else
                        {
                            if (_newPINFirstEntry != pinDisplay.Text)
                            {
                                _newPINFirstEntry = "";
                                pinDisplay.Text = "";
                                await DisplayAlert("", "your new pin and confirmation did not match, please try again", "OK");
                                return;
                            }
                            else
                            {
                                var wasPinUpdated = false;

                                if (_userType == UserType.Parent)
                                    wasPinUpdated = await App.Database.UpdateParentPIN(_userPersonId.Value, _parentNameStart, _originalPIN, pinDisplay.Text);
                                else if (_userType == UserType.Employee)
                                    wasPinUpdated = await App.Database.UpdateEmployeePIN(_userPersonId.Value, _originalPIN, pinDisplay.Text);

                                if (wasPinUpdated)
                                {
                                    await DisplayAlert("", "your pin has been updated", "OK");

                                    await Navigation.PushAsync(new PreCheckInPage(_userType, _userPersonId.Value, _userFirstName, _userLastName), false);
                                }
                                else
                                {
                                    await DisplayAlert("", "there was a problem while updating your pin, please seek assistance", "OK");
                                    await Helpers.Navigation.ResetNavigationAndGoToRoot();
                                }

                                return;
                            }
                        }
                    }

                    _attemptCount++;

                    if (_userType == UserType.Parent)
                    {
                        var parentMatches = await App.Database.AuthenticateParent(_parentNameStart, pinDisplay.Text);

                        // if no match was found, it is possible pin was changed and system hasn't sync'd yet - so we try auth via server
                        if ((parentMatches == null || !parentMatches.Any()) && _attemptCount < MAX_ATTEMPTS && Connectivity.NetworkAccess == NetworkAccess.Internet)
                        {
                            var msg = new ModalUserMessage("processing...", true, true);

                            try
                            {
                                msg.Show();
                                var parentArray = await new RestService().AuthenticateParent(_parentNameStart, pinDisplay.Text);

                                if (parentArray != null)
                                {
                                    var serverParentMatches = parentArray.ToList<Parent>();

                                    foreach (var parent in serverParentMatches)
                                        await App.Database.UpdateParent(parent);

                                    parentMatches = serverParentMatches;
                                }
                            }
                            catch (Exception ex)
                            {
                                Helpers.Logging.Log(ex);
                            }
                            finally
                            {
                                msg.Close();
                            }
                        }

                        if (parentMatches == null || !parentMatches.Any())
                        {
                            if (_attemptCount >= MAX_ATTEMPTS)
                            {
                                // can't lock since we don't know the parent yet.  just taking back to main for now after max attempts.
                                await DisplayAlert("", "invalid pin", "OK");
                                await Helpers.Navigation.ResetNavigationAndGoToRoot();
                            }
                            else
                            {
                                await DisplayAlert("", "invalid pin, please try again", "OK");
                            }
                        }
                        else if (parentMatches.Count == 1)
                        {
                            var parent = parentMatches.First();
                            _userPersonId = parent.PersonID;
                            _userFirstName = parent.FN;
                            _userLastName = parent.LN;

                            if (parent.ResetPIN.HasValue && parent.ResetPIN.Value.Date <= DateTime.Today)
                            {
                                _originalPIN = pinDisplay.Text;
                                _setPINMode = true;
                                pinDisplay.Text = "";

                                await DisplayAlert("", "your pin has been reset, you are required to change it, enter your new pin", "OK");
                                return;
                            }

                            await Navigation.PushAsync(new PreCheckInPage(UserType.Parent, parent.PersonID, parent.FN, parent.LN), false);
                        }
                        else
                        {
                            //per GSI we now want multiple matches to generate an error message
                            //var page = new ParentSelectionPage();
                            //var viewModel = (ParentSelectionPageViewModel)page.BindingContext;
                            //viewModel.Parents = parentMatches;
                            //viewModel.PIN = pinDisplay.Text;

                            //await Navigation.PushAsync(page, false);   

                            await DisplayAlert("", "you are not using a unique PIN, please contact your school administrator to reset your PIN", "OK");
                            return;
                        }

                        pinDisplay.Text = "";
                        return;
                    }
                    else if (_userType == UserType.Employee)
                    {
                        var employee = await App.Database.AuthenticateEmployee(_userPersonId.Value, pinDisplay.Text);

                        if (employee == null && _attemptCount < MAX_ATTEMPTS && Connectivity.NetworkAccess == NetworkAccess.Internet)
                        {
                            var msg = new ModalUserMessage("processing...", true, true);

                            try
                            {
                                msg.Show();
                                var serverEmployee = await new RestService().AuthenticateEmployee(_userPersonId.Value, pinDisplay.Text);

                                if (serverEmployee != null)
                                {
                                    await App.Database.UpdateEmployee(serverEmployee);
                                    employee = serverEmployee;
                                }
                            }
                            catch (Exception ex)
                            {
                                Helpers.Logging.Log(ex);
                            }
                            finally
                            {
                                msg.Close();
                            }
                        }

                        if (employee == null)
                        {
                            if (_attemptCount >= MAX_ATTEMPTS)
                            {
                                try
                                {
                                    await App.Database.LockEmployeePIN(_userPersonId.Value);
                                }
                                catch (Exception ex)
                                {
                                    Helpers.Logging.Log(ex);
                                }

                                await DisplayAlert("", "your pin has been locked due to too many invalid attempts, please seek assistance", "OK");
                                pinDisplay.Text = "";

                                await Helpers.Navigation.ResetNavigationAndGoToRoot();

                                return;
                            }
                            else
                            {
                                await DisplayAlert("", "invalid pin, please try again", "OK");
                                pinDisplay.Text = "";
                                return;
                            }
                        }
                        else
                        {
                            if (employee.LockedPIN)
                            {
                                await DisplayAlert("", "your pin has been locked due to too many invalid attempts, please seek assistance", "OK").ContinueWith(t =>
                                    {
                                        Device.BeginInvokeOnMainThread(async () =>
                                        {
                                            await Helpers.Navigation.ResetNavigationAndGoToRoot();
                                        });
                                    });

                                pinDisplay.Text = "";
                                return;
                            }

                            _userFirstName = employee.FN;
                            _userLastName = employee.LN;

                            if (employee.ForceResetPIN.HasValue && employee.ForceResetPIN.Value.Date <= DateTime.Today)
                            {
                                _originalPIN = pinDisplay.Text;
                                _setPINMode = true;
                                pinDisplay.Text = "";

                                await DisplayAlert("", "your pin has been reset, you are required to change it, enter your new pin", "OK");
                                return;
                            }

                            pinDisplay.Text = "";

                            // if we made it here, pin was correct and employee can proceed
                            await Navigation.PushAsync(new PreCheckInPage(UserType.Employee, employee.PersonID, employee.FN, employee.LN), false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TimeClock.Helpers.Logging.Log(ex);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Helpers.Navigation.ResetNavigationAndGoToRoot();
                    });
                }
            });
        }

        protected void DigitButtonClick(object sender, EventArgs e)
        {
            var button = (Button)sender;

            if (button.StyleId.ToLower() == "clear")
            {
                pinDisplay.Text = "";
            }
            else if (pinDisplay.Text == null || pinDisplay.Text.Length < MAX_DIGITS)
            {
                pinDisplay.Text += button.StyleId;
            }
        }
    }
}
