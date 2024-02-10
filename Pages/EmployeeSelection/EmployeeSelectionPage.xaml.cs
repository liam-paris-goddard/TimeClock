using TimeClock.Controls;
using TimeClock.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TimeClock
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EmployeeSelectionPage : TimedContentPage
    {
        public EmployeeSelectionPage()
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

            Navigation.PushAsync(new PinPadPage(UserType.Employee, e.SelectedText, personId), false);
        }
    }
}
