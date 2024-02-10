using System;
using Microsoft.Maui.Controls;

namespace TimeClock.Controls
{
    public class GoddardButton : Button
    {
        public static readonly BindableProperty UseAltColorProperty = BindableProperty.Create(nameof(UseAltColor), typeof(bool), typeof(GoddardButton), false);

        public bool UseAltColor
        {
            get => (bool)GetValue(UseAltColorProperty);
            set => SetValue(UseAltColorProperty, value);
        }

        public GoddardButton()
        {
            this.Clicked += GoddardButton_Clicked;
        }

        private void GoddardButton_Clicked(object? sender, EventArgs e)
        {
            GlobalResources.Current.UpdateLastUserInteraction();
        }
    }
}