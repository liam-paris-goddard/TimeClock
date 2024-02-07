using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TimeClock
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserMessagePage : ContentPage
    {
        public UserMessagePage(string message, bool showActivityIndicator)
        {
            InitializeComponent();
            messageLabel.Text = message;
            activityIndicator.IsRunning = showActivityIndicator;
            activityIndicator.IsVisible = showActivityIndicator;
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}