using Goddard.Clock.ViewModels;
using Microsoft.Maui.Primitives;

namespace Goddard.Clock;
public partial class LoginPage : UntimedContentPage
{
    [Obsolete]
    public LoginPage(LoginPageViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        username.ReturnButton = Controls.ReturnButtonType.Next;
        username.NextView = password;

        password.ReturnButton = Controls.ReturnButtonType.Next;
        password.NextView = username;
        loginButton.Margin = DeviceInfo.Idiom == DeviceIdiom.Phone ? new Thickness(0, 10, 0, 0) : new Thickness(170, 10, 170, 0);
        loginFrame.Margin = DeviceInfo.Idiom == DeviceIdiom.Phone ? new Thickness(0) : new Thickness(100);
        loginFrame.VerticalOptions = DeviceInfo.Idiom == DeviceIdiom.Phone ? LayoutOptions.FillAndExpand : LayoutOptions.StartAndExpand;
    }

    protected override void OnAppearing()
    {
        if (string.IsNullOrWhiteSpace(Helpers.Settings.Username))
            _ = username.Focus();
        else
            _ = password.Focus();
    }
}
