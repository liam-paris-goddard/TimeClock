using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TimeClock
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UntimedContentPage : ContentPage
    {
        public UntimedContentPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            GlobalResources.Current.GoToMainOnPageTimeout = false;
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}
