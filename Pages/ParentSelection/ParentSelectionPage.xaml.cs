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
    public partial class ParentSelectionPage : TimedContentPage
    {
        public ParentSelectionPage()
        {
            InitializeComponent();
        }

        protected void PagedGoddardButtonGridButtonClick(object sender, PagedGoddardButtonGrid.ButtonClickEventArgs e)
        {
            long personId = 0;

            if (!Int64.TryParse(e.SelectedValue, out personId))
            {
                //TODO: error.  or log.  or something.
            }

            var pin = ((ParentSelectionPageViewModel)this.BindingContext).PIN;

            App.Database.AuthenticateParent(personId, pin).ContinueWith((t) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    var parent = t.Result;

                    if (parent.ResetPIN.HasValue && parent.ResetPIN.Value <= DateTime.Now)
                        Navigation.PushAsync(new PinPadPage(UserType.Parent, e.SelectedText, personId, true, pin, true), false);
                    else
                        Navigation.PushAsync(new PreCheckInPage(UserType.Parent, personId, parent.FN, parent.LN), false);
                });
            });
        }
    }
}
