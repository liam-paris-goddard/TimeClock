using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using TimeClock;
namespace TimeClock
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimedContentPage : ContentPage
    {

        protected override void OnAppearing()
        {
            base.OnAppearing();
            GlobalResources.Current.GoToMainOnPageTimeout = true;
            GlobalResources.Current.UpdateLastUserInteraction();
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}