using TimeClock.Controls;
using TimeClock.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TimeClock
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PreCheckInPage : TimedContentPage
    {
        private long _userPersonId;
        private string _userFirstName;
        private string _userLastName;
        private long? _employeeSelectedFamilyId;
        private UserType _userType;
        List<CheckInSelector> _checkInSelectors = new List<CheckInSelector>();

        public PreCheckInPage(UserType personType, long userPersonId, string userFirstName, string userLastName, long? employeeSelectedFamilyId = null)
        {
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
                    App.Database.GetChildrenForFamily(_employeeSelectedFamilyId.Value).ContinueWith((childrenTask) =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
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

                    App.Database.CanEmployeeCheckChildInOut(_userPersonId).ContinueWith((t) =>
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                footer.ShowCenterButton = t.Result;
                            });
                        });

                    //note SWH 2019-05-03 - now we can default hide in xaml so this is not needed but preserving for reference
                    //// hide all but first - note that we can't default to hidden in xaml and then unhide here due to iOS issue 
                    //// (doesn't appear to call Draw when we try to show a previously hidden button).
                    //for (int i = 1; i < _checkInSelectors.Count; i++)
                    //    _checkInSelectors[i].IsVisible = false;
                }
            }
            else if (_userType == UserType.Parent)
            {
                App.Database.GetChildrenForParent(_userPersonId).ContinueWith((childrenTask) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
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
                    selector.ImageType = child.Sex.ToLower().Trim() == "f" ? CheckInSelector.CheckInSelectorImageType.Girl : CheckInSelector.CheckInSelectorImageType.Boy;
                }
                else
                {
                    selector.IsVisible = false;
                }
            }
        }

        protected void SelectionMadeHandler(object sender, EventArgs e)
        {
            var activeSelectors = _checkInSelectors.Where(s => s.IsActive);

            if (activeSelectors.All(s => s.SelectedEventType.HasValue) && (activeSelectors.All(s => s.SelectedEventType == ClockEventType.In) || activeSelectors.All(s => s.SelectedEventType == ClockEventType.Out)))
                // all of the selectors have been set the same way.  automatically proceed.
                okFooterButtonClick(null, null);
        }

        protected void okFooterButtonClick(object sender, EventArgs e)
        {
            try
            {
                var checkInInfo = new List<EventExtended>();

                if (_userType == UserType.Employee && !_employeeSelectedFamilyId.HasValue)
                {
                    if (!checkInSelector1.SelectedEventType.HasValue)
                    {
                        DisplayAlert("", "please make a selection", "OK");
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
                        DisplayAlert("", "please make a selection", "OK");
                        return;
                    }

                    foreach (var checkInSelector in _checkInSelectors.Where(s => s.IsActive && s.SelectedEventType.HasValue))
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

                Navigation.PushAsync(new CheckInPage(checkInInfo), false);
            }
            catch (Exception ex)
            {
                TimeClock.Helpers.Logging.Log(ex);
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Helpers.Navigation.ResetNavigationAndGoToRoot();
                });
            }
        }

        protected void childModeFooterButtonClick(object sender, EventArgs e)
        {
            if (_userType == UserType.Employee)
            {
                App.EmployeeUserPersonID = _userPersonId;
                Navigation.PushAsync(new HomePage(), false);
            }
        }

        private void checkInAllButton_Clicked(object sender, EventArgs e)
        {
            foreach (var selector in _checkInSelectors.Where(s => s.IsActive))
                selector.ChangeSelectedEventType(ClockEventType.In);

            okFooterButtonClick(null, null);
        }

        private void checkOutAllButton_Clicked(object sender, EventArgs e)
        {
            foreach (var selector in _checkInSelectors.Where(s => s.IsActive))
                selector.ChangeSelectedEventType(ClockEventType.Out);

            okFooterButtonClick(null, null);
        }
    }
}
