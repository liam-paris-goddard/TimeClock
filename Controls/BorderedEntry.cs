using System;
using Microsoft.Maui.Controls;

namespace TimeClock.Controls
{
    public enum ReturnButtonType
    {
        None,
        Next
    }

    public class BorderedEntry : Entry
    {
        public BorderedEntry()
        {
            this.TextChanged += BorderedEntry_TextChanged;
        }

        private void BorderedEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            GlobalResources.Current.UpdateLastUserInteraction();
        }

        public static readonly BindableProperty ReturnButtonProperty =
            BindableProperty.Create("ReturnButton", typeof(ReturnButtonType), typeof(BorderedEntry), ReturnButtonType.None);

        public ReturnButtonType ReturnButton
        {
            get => (ReturnButtonType)GetValue(ReturnButtonProperty);
            set => SetValue(ReturnButtonProperty, value);
        }

        public static readonly BindableProperty NextViewProperty =
            BindableProperty.Create("NextView", typeof(View), typeof(BorderedEntry));

        public View NextView
        {
            get => (View)GetValue(NextViewProperty);
            set => SetValue(NextViewProperty, value);
        }

        public void OnNext()
        {
            NextView?.Focus();
        }
    }
}