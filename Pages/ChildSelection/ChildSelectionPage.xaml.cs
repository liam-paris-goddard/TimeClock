using TimeClock.Controls;
using TimeClock.Models;
using System;
using System.Linq;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Collections.Generic;

namespace TimeClock
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChildSelectionPage : TimedContentPage
    {
        public ChildSelectionPage()
        {
            InitializeComponent();
        }

        protected void PagedGoddardButtonGridButtonClick(object sender, PagedGoddardButtonGrid.ButtonClickEventArgs e)
        {
            long familyId = 0;

            if (!Int64.TryParse(e.SelectedValue, out familyId))
            {
                //TODO: error.  or log.  or something.
            }

            var viewModel = (ChildSelectionPageViewModel)BindingContext;

            if (!viewModel.EmployeeUserPersonId.HasValue)
            {
                //TODO: error/log
            }

            Navigation.PushAsync(new PreCheckInPage(UserType.Employee, viewModel.EmployeeUserPersonId.Value, viewModel.EmployeeUserFN, viewModel.EmployeeUserLN, familyId));
        }
    }
}
