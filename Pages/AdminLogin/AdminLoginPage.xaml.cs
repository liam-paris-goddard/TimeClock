using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TimeClock
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdminLoginPage : TimedContentPage
    {
        public AdminLoginPage()
        {
            InitializeComponent();

            passwordField.Completed += PasswordField_Completed;
        }
        private void PasswordField_Completed(object sender, EventArgs e)
        {
            verifyButton.Command.Execute(null);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            passwordField.Focus();
        }
    }
}
