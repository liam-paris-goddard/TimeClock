using TimeClock.Controls;
using TimeClock.Data;
using TimeClock.Helpers;
using TimeClock.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace TimeClock
{
    public partial class HomePage : UntimedContentPage
    {
        public HomePage()
        {
            InitializeComponent();

            letterButtons.LetterButtonClickHandler += new EventHandler<LetterButtons.LetterButtonClickEventArgs>(LetterButtonClick);

            if (App.EmployeeUserPersonID.HasValue)
            {
                letterButtons.Title = "CHILD'S LAST NAME";
                letterButtons.IsTypingMode = false;
                footer.ShowCenterButton = true;
            }
            else
            {
                footer.ShowCenterButton = false;
                letterButtons.TypingModeSubmitButtonClickHandler += new EventHandler<LetterButtons.TypingModeSubmitButtonClickEventArgs>(LetterButtonsTypingModeSubmitButtonClick);
                letterButtons.Title = "LAST NAME";
                letterButtons.IsTypingMode = true;
                letterButtons.TypingModeMaxLength = 3;
                letterButtons.TypingModeMinLength = 3;
            }

            //TODO: not sure if this is best place for this as this page gets instantiated many times
            //but this is also the one place where we know the credentials have been supplied and school
            //selected and will always be seen, since Start on an already Started engine is a non-op, it won't
            //hurt to have it here for sure
            SyncEngine.Instance.Start();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            letterButtons.ClearTypingModeText();

            if (App.EmployeeUserPersonID.HasValue)
            {
                // this is by default an untimed page, but for employee/child mode we still want to go to normal version of main page when inactive.
                GlobalResources.Current.GoToMainOnPageTimeout = true;
            }
        }

        protected async void LetterButtonClick(object sender, LetterButtons.LetterButtonClickEventArgs e)
        {
            if (e.IsEmployeeButton)
            {
                await this.Navigation.PushAsync(new EmployeeSelectionPage(), false);
                return;
            }

            if (App.EmployeeUserPersonID.HasValue)
            {
                var children = await App.Database.GetChildList(e.SelectedLetter);

                var page = new ChildSelectionPage();
                var viewModel = (ChildSelectionPageViewModel)page.BindingContext;
                viewModel.Children = children;
                viewModel.EmployeeUserPersonId = App.EmployeeUserPersonID;

                await this.Navigation.PushAsync(page, false);
            }

            // if not employee mode then the LetterButtons control should be in typing mode and normal letter buttons are just used 
            // by the user control itself to enter text and don't need to be handled here.
        }

        protected async void LetterButtonsTypingModeSubmitButtonClick(object sender, LetterButtons.TypingModeSubmitButtonClickEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(e.EnteredText))
                return;

            if (e.EnteredText.Length < 2)
            {
                await DisplayAlert("", "please enter the first three letters of your last name", "OK");
                return;
            }

            await Navigation.PushAsync(new PinPadPage(UserType.Parent, e.EnteredText), false);
        }
    }
}
