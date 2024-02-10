using TimeClock.Controls;
using System.Linq;
using Microsoft.Maui.Controls;

namespace TimeClock
{
    public partial class StateSelectionPage : TimedContentPage
    {
        public StateSelectionPage()
        {
            InitializeComponent();
        }

        protected void PagedGoddardButtonGridButtonClick(object sender, PagedGoddardButtonGrid.ButtonClickEventArgs e)
        {
            var schoolsForSelectedState = ((StateSelectionPageViewModel)BindingContext).GetSchoolsByState(e.SelectedValue).ToList();

            var page = new SchoolSelectionPage();
            ((SchoolSelectionPageViewModel)page.BindingContext).Schools = schoolsForSelectedState;

            Navigation.PushAsync(page, false);
        }
    }
}
