using Goddard.Clock.Models;
using Goddard.Clock.Data;
using Goddard.Clock.Factories;
using Goddard.Clock.Helpers;

namespace Goddard.Clock;
public partial class PinPadPage : TimedContentPage
{
    private const int MAX_ATTEMPTS = 3;
    private const int MIN_DIGITS = 4;
    private const int MAX_DIGITS = 8;

    private readonly UserType _userType;
    private long? _userPersonId = null;
    private string _userFirstName = "";
    private string _userLastName = "";
    private int _attemptCount = 0;
    private bool _setPINMode = false;
    private string _originalPIN = "";
    private string _newPINFirstEntry = "";
    private bool _showPinResetMessageOnLoad = false;
    private readonly string _parentNameStart = "";

    public double _pinPadFontSize = 30;
    public double PinPadFontSize
    {
        get { return _pinPadFontSize; }
        set
        {
            if (_pinPadFontSize != value)
            {
                _pinPadFontSize = value;
                OnPropertyChanged();
            }
        }
    }

    public double _PinPadButtonSize = 100;
    public double PinPadButtonSize
    {
        get { return _PinPadButtonSize; }
        set
        {
            if (_PinPadButtonSize != value)
            {
                _PinPadButtonSize = value;
                OnPropertyChanged();
            }
        }
    }

    private readonly ClockDatabase? _database;
    private readonly NavigationService? _navigation;
    private readonly SyncEngineService _syncEngine;
    private readonly IServiceProvider _serviceProvider;
    public PinPadPage(IServiceProvider serviceProvider, UserType userType, string personName, long? personId = null, bool setPINMode = false, string originalPIN = "", bool showPinResetMessageOnLoad = false)
    {
        _serviceProvider = serviceProvider;
        _database = App.Database ?? throw new ArgumentNullException(nameof(App.Database));
        _syncEngine = App.SyncEngine ?? throw new ArgumentNullException(nameof(App.SyncEngine));;
        _navigation = App.NavigationService;
        BindingContext = this;
        SetResponsiveVars();
        InitializeComponent();

        _userType = userType;
        _userPersonId = personId;
        _setPINMode = setPINMode;
        _originalPIN = originalPIN;
        _showPinResetMessageOnLoad = showPinResetMessageOnLoad;


        if (userType != UserType.Employee)
        {
            _parentNameStart = personName;
        }

        footer.CenterText = personName;
    }

    protected override async void OnAppearing()
    {
        if(!_navigation.isFromModal) {
            _ = await _syncEngine.ForcePull();
            _ = await _syncEngine.ForceSend();
        }
        _navigation.isFromModal = false;
        base.OnAppearing();
        if (_showPinResetMessageOnLoad)
        {
            await Task.Delay(500);

            await DisplayAlert("", "your pin has been reset, you are required to change it, enter your new pin", "OK");
        }
        if (_userType == UserType.Employee)
        {
            if (!_userPersonId.HasValue)
            {
                if (_database != null)
                {
                    await Logging.Log(_database, "Error - Pin Pad - null user ID");
                }
                await Task.Delay(500);

                await DisplayAlert("Error", "an error has occurred while attempting to load the pin pad", "OK");
                if (_navigation != null)
                {
                    if (_navigation != null) { await _navigation.ResetNavigationAndGoToRoot(); }
                }
            }
        }
    }

    protected override void OnDeviceInformationChanged(string propertyName)
    {
        if (propertyName == "DeviceWidth" || propertyName == "GlobalOrientation" || propertyName == "DeviceHeight")
        {
            SetResponsiveVars();
        }

    }

    private void SetResponsiveVars()
    {
        PinPadFontSize = 30;
        PinPadButtonSize = 100;
        if (DeviceType == "small")
        {
            if (DeviceOrientation == DisplayOrientation.Portrait)
            {
            }
            else
            {
                PinPadFontSize = 25;//20;

                PinPadButtonSize = 80;//50;
            }
        }
        else if (DeviceType == "medium")
        {
            if (DeviceOrientation == DisplayOrientation.Portrait)
            {
                PinPadFontSize = 25;
                PinPadButtonSize = 80;
            }
            else
            {
                PinPadFontSize = 25;
                PinPadButtonSize = 80;
            }
        }
    }


    private void Footer_RightButtonClickHandler(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                if (_setPINMode)
                {
                    if (!_userPersonId.HasValue)
                    {
                        if (_database != null)
                        {
                            await Logging.Log(_database, "Error - Pin Pad - Null user ID");
                        }

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
                            var isValid = false;
                            if (_database != null)
                            {
                                isValid = await _database.CheckUniquenessPIN(_userPersonId.Value, _parentNameStart, pinDisplay.Text);
                            }
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

                            if(_database != null){if (_userType == UserType.Parent)
                            {
                                wasPinUpdated = await _database.UpdateParentPIN(_userPersonId.Value, _parentNameStart, _originalPIN, pinDisplay.Text);
                            }
                            else if (_userType == UserType.Employee)
                            {
                                wasPinUpdated = await _database.UpdateEmployeePIN(_userPersonId.Value, _originalPIN, pinDisplay.Text);
                            }}

                            if (wasPinUpdated)
                            {

                                await DisplayAlert("", "your pin has been updated", "OK");
                                _ = await _syncEngine.ForceSend();
                                var factory = _serviceProvider.GetRequiredService<IPreCheckInPageFactory>();
                                var page = factory.Create(_userType, _userPersonId.Value, _userFirstName, _userLastName);
                                await Navigation.PushAsync(page, false);
                            }
                            else
                            {

                                await DisplayAlert("", "there was a problem while updating your pin, please seek assistance", "OK");
                                if (_navigation != null) { await _navigation.ResetNavigationAndGoToRoot(); }
                            }

                            return;
                        }
                    }
                }

                _attemptCount++;

                if (_userType == UserType.Parent)
                {
                    var parentMatches = _database != null ? await _database.AuthenticateParent(_parentNameStart, pinDisplay.Text) : null;

                    // if no match was found, it is possible pin was changed and system hasn't sync'd yet - so we try auth via server
                    if ((parentMatches == null || !parentMatches.Any()) && _attemptCount < MAX_ATTEMPTS && Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        var msg = new ModalUserMessage(_navigation, _database, "processing...", true, true);

                        try
                        {
                            msg.Show();
                            var parentArray = await new RestService().AuthenticateParent(_parentNameStart, pinDisplay.Text);

                            if (parentArray != null)
                            {
                                var serverParentMatches = parentArray.ToList<Parent>();

                                foreach (var parent in serverParentMatches)
                                {
                                    if(_database != null)
                                        _ = await _database.UpdateParent(parent);
                                }

                                parentMatches = serverParentMatches;
                            }
                        }
                        catch (Exception ex)
                        {
                            _ = Logging.Log(_database, ex);
                        }
                        finally
                        {
                            await msg.Close();
                        }
                    }

                    if (parentMatches == null || !parentMatches.Any())
                    {
                        if (_attemptCount >= MAX_ATTEMPTS)
                        {
                            // can't lock since we don't know the parent yet.  just taking back to main for now after max attempts.

                            await DisplayAlert("", "invalid pin", "OK");
                            if (_navigation != null) { await _navigation.ResetNavigationAndGoToRoot(); }
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

                        var factory = _serviceProvider.GetRequiredService<IPreCheckInPageFactory>();
                        var page = factory.Create(UserType.Parent, (long)parent.PersonID, parent.FN, parent.LN);
                        await Navigation.PushAsync(page, false);
                    }
                    else
                    {
                        await DisplayAlert("", "you are not using a unique PIN, please contact your school administrator to reset your PIN", "OK");
                        return;
                    }

                    pinDisplay.Text = "";
                    return;
                }
                else if (_userType == UserType.Employee)
                {
                    Employee? employee = null;
if(_database != null){                    employee = await _database.AuthenticateEmployee(_userPersonId.Value, pinDisplay.Text);
}
                    if (employee == null && _attemptCount < MAX_ATTEMPTS && Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        var msg = new ModalUserMessage(_navigation, _database, "processing...", true, true);

                        try
                        {
                            msg.Show();
                            var serverEmployee = await new RestService().AuthenticateEmployee(_userPersonId.Value, pinDisplay.Text);

                            if (serverEmployee != null)
                            {
                                _ = await _database.UpdateEmployee(serverEmployee);
                                employee = serverEmployee;
                            }
                        }
                        catch (Exception ex)
                        {
                            await Logging.Log(_database, ex);
                        }
                        finally
                        {
                            await msg.Close();
                        }
                    }

                    if (employee == null)
                    {
                        if (_attemptCount >= MAX_ATTEMPTS)
                        {
                            try
                            {
                                if(_database != null && _userPersonId.HasValue)
                                    _ = await _database.LockEmployeePIN(_userPersonId.Value);
                            }
                            catch (Exception ex)
                            {
                                await Logging.Log(_database, ex);
                            }


                            await DisplayAlert("", "your pin has been locked due to too many invalid attempts, please seek assistance", "OK");
                            pinDisplay.Text = "";

                            if (_navigation != null) { await _navigation.ResetNavigationAndGoToRoot(); }

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
                            // TODO: test to make sure both are running 

                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                _ = await DisplayAlert("", "your pin has been locked due to too many invalid attempts, please seek assistance", "OK").ContinueWith(async (task) =>
                                                            {
                                                                if (_navigation != null) { await _navigation.ResetNavigationAndGoToRoot(); }
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
                        var factory = _serviceProvider.GetRequiredService<IPreCheckInPageFactory>();
                        var page = factory.Create(UserType.Employee, (long)employee.PersonID, employee.FN, employee.LN);
                        // if we made it here, pin was correct and employee can proceed
                        await Navigation.PushAsync(page, false);
                    }
                }
            }
            catch (Exception ex)
            {
                await Logging.Log(_database, ex);
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (_navigation != null) { await _navigation.ResetNavigationAndGoToRoot(); }
                });
            }
        });
    }

    protected void DigitButtonClick(object? sender, EventArgs e)
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
