using Goddard.Clock.Controls;
using Goddard.Clock.Models;
using Goddard.Clock.Data;
using Goddard.Clock.Factories;
using Goddard.Clock.Helpers;
using Microsoft.ApplicationInsights;

namespace Goddard.Clock;
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PreCheckInPage : TimedContentPage
{
    private readonly ClockDatabase _database = App.Database ?? throw new ArgumentNullException(nameof(App.Database));
    private readonly TelemetryClient _telemetryClient;
    private long _userPersonId;
    private string _userFirstName;
    private string _userLastName;
    private long? _employeeSelectedFamilyId;
    private UserType _userType;
        List<CheckInSelector> _checkInSelectors = new List<CheckInSelector>();


    private readonly IServiceProvider _serviceProvider;
    private readonly NavigationService _navigation = App.NavigationService ?? throw new ArgumentNullException(nameof(App.NavigationService));

    public PreCheckInPage(UserType personType, long userPersonId, string userFirstName, string userLastName, IServiceProvider serviceProvider, long? employeeSelectedFamilyId = null)
    {
        _serviceProvider = serviceProvider;
        _telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();
        InitializeComponent();

                    _checkInSelectors = new List<CheckInSelector>()
            {
                checkInSelector1,
                checkInSelector2,
                checkInSelector3,
                checkInSelector4,
                checkInSelector5,
                checkInSelector6
            };

            foreach (var checkInSelector in _checkInSelectors)
                checkInSelector.SelectionMade += new EventHandler(SelectionMadeHandler);

        _userPersonId = userPersonId;
        _userType = personType;
        _userFirstName = userFirstName;
        _userLastName = userLastName;
        _employeeSelectedFamilyId = employeeSelectedFamilyId;

        if (_userType == UserType.Employee)
        {
            // if a family ID, employee is checking in/out children.  if not, employee is checking in/out theirself.
            if (_employeeSelectedFamilyId.HasValue)
            {
                _ = _database.GetChildrenForFamily(_employeeSelectedFamilyId.Value).ContinueWith((childrenTask) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        footer.ShowCenterButton = false;
                        var children = childrenTask.Result;
                        SetUpChildSelectors(children);
                    });
                });
            }
            else
            {
                    checkInAllButton.IsVisible = false;
                    checkOutAllButton.IsVisible = false;

                    checkInSelector1.FirstName = _userFirstName;
                    checkInSelector1.LastName = _userLastName;
                    checkInSelector1.PersonId = _userPersonId;
                    checkInSelector1.ImageType = CheckInSelector.CheckInSelectorImageType.Employee;
                    checkInSelector1.IsVisible = true;
                _ = _database.CanEmployeeCheckChildInOut(_userPersonId).ContinueWith((t) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            footer.ShowCenterButton = t.Result;
                        });
                    });
            }
        }
        else if (_userType == UserType.Parent)
        {
            _ = _database.GetChildrenForParent(_userPersonId).ContinueWith((childrenTask) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    footer.ShowCenterButton = false;
                    var children = childrenTask.Result;
                    SetUpChildSelectors(children);
                });
            });
        }
    }

         private void SetUpChildSelectors(List<Child> children)
    {   
         if (children.Count < 2)
                     {
                //note - doing it this way because defaulting these to not visible and showing later can cause iOS to never draw them
                checkInAllButton.IsVisible = false;
                checkOutAllButton.IsVisible = false;
            }

            for (var i = 0; i < _checkInSelectors.Count; i++)
{
                var selector = _checkInSelectors[i];
                if (i < children.Count)
                {
                    var child = children[i];
                    selector.IsVisible = true;
                    selector.FirstName = child.FN;
                    selector.LastName = child.LN;
                    selector.PersonId = child.PersonID;
                    selector.Classroom = child.Classroom.ToUpper();
                    if(child.Sex.ToLower().Trim() == "f") {
                        selector.ImageType = CheckInSelector.CheckInSelectorImageType.Girl;
                    } else if (child.Sex.ToLower().Trim() == "employee") {
                        selector.ImageType = CheckInSelector.CheckInSelectorImageType.Employee;
                    } else {
                                                selector.ImageType = CheckInSelector.CheckInSelectorImageType.Boy;

                    }
                }
                else
                {
                    selector.IsVisible = false;
                }
    }
    }

    protected void SelectionMadeHandler(object? sender, EventArgs e)
    {
            var activeSelectors = _checkInSelectors.Where(s => s.IsActive);

        if (activeSelectors.All(s => s.SelectedEventType.HasValue) && (activeSelectors.All(s => s.SelectedEventType == ClockEventType.In) || activeSelectors.All(s => s.SelectedEventType == ClockEventType.Out))){
            okFooterButtonClick(null, null);
        }            // all of the selectors have been set the same way.  automatically proceed.
    }

   protected void okFooterButtonClick(object? sender, EventArgs? e)
{
    try
    {
        var checkInInfo = new List<EventExtended>();

        if (_userType == UserType.Employee && !_employeeSelectedFamilyId.HasValue)
        {
            if (!checkInSelector1.SelectedEventType.HasValue)
            {
                _ = DisplayAlert("", "please make a selection", "OK");
                return;
            }

            checkInInfo.Add(new EventExtended()
            {
                UserType = UserType.Employee,
                Type = checkInSelector1.SelectedEventType.Value,
                Occurred = DateTime.Now,
                TargetPersonID = checkInSelector1.PersonId,
                TargetPersonName = checkInSelector1.FullName,
                UserPersonID = _userPersonId,
                UserPersonName = $"{_userLastName}, {_userFirstName}"
            });
        }
        else
        {
            if (!_checkInSelectors.Any(c => c.IsActive && c.SelectedEventType.HasValue))
            {
                _ = DisplayAlert("", "please make a selection", "OK");
                return;
            }

            foreach (var checkInSelector in _checkInSelectors.Where(s => s.IsActive && s.SelectedEventType.HasValue))
            {
                if (checkInSelector != null && checkInSelector.SelectedEventType.HasValue && checkInSelector.IsActive)
                {
                    checkInInfo.Add(new EventExtended()
                    {
                        UserType = _employeeSelectedFamilyId.HasValue ? UserType.InLocoParentis : UserType.Parent,
                        Type = checkInSelector.SelectedEventType.Value,
                        Occurred = DateTime.Now,
                        TargetPersonID = checkInSelector.PersonId,
                        TargetPersonName = checkInSelector.FullName,
                        UserPersonID = _userPersonId,
                        UserPersonName = $"{_userLastName}, {_userFirstName}"
                    });
                }
            }
        }


        // Navigate to the next page
        foreach (var checkIn in checkInInfo)
        {
            _telemetryClient.TrackEvent("Check-In/Out", new Dictionary<string, string> { { "UserType", checkIn.UserType.ToString() }, { "EventType", checkIn.Type.ToString() }, { "TargetPersonID", checkIn.TargetPersonID.ToString() }, { "UserPersonID", checkIn.UserPersonID.ToString() } });
            _telemetryClient.Flush();
        }
            
        _ = Navigation.PushAsync(new CheckInPage(checkInInfo), false);

        // Reset the selection types after navigating to avoid triggering the alert
        foreach (var checkInSelector in _checkInSelectors)
        {
            if (checkInSelector != null)
            {
                checkInSelector.SelectedEventType = null;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Exception: " + ex.Message);
        _ = Logging.Log(_database, ex);
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (_navigation != null) { await _navigation.ResetNavigationAndGoToRoot(); }
        });
    }
}
 protected void childModeFooterButtonClick(object? sender, EventArgs e)
    {
        if (_userType == UserType.Employee)
        {
            App.EmployeeUserPersonId = _userPersonId;
            var factory = _serviceProvider.GetRequiredService<IHomePageFactory>();
            var page = factory.Create(false);
            _ = Navigation.PushAsync(page, false);
        }
    }

private void checkInAllButton_Clicked(object? sender, EventArgs e)
{
    foreach (var selector in _checkInSelectors.Where(s => s.IsActive))
    {
        selector.ChangeSelectedEventType(ClockEventType.In);
    }
}

private void checkOutAllButton_Clicked(object? sender, EventArgs e)
{
    foreach (var selector in _checkInSelectors.Where(s => s.IsActive))
    {
        selector.ChangeSelectedEventType(ClockEventType.Out);
    }
}
 
}
