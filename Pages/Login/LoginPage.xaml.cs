using Microsoft.Maui.Controls;

namespace TimeClock
{
    public partial class LoginPage : UntimedContentPage
    {
        public LoginPage()
        {
            InitializeComponent();

            username.ReturnButton = Controls.ReturnButtonType.Next;
            username.NextView = password;

            password.ReturnButton = Controls.ReturnButtonType.Next;
            password.NextView = username;
        }

        protected override void OnAppearing()
        {
            if (string.IsNullOrWhiteSpace(Helpers.Settings.Username))
                username.Focus();
            else
                password.Focus();
        }
    }
}
