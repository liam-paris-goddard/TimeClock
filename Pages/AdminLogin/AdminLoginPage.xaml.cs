using Goddard.Clock.ViewModels;
namespace Goddard.Clock;
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AdminLoginPage : TimedContentPage
{
    [Obsolete]
    public AdminLoginPage(AdminLoginPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        passwordField.Completed += PasswordField_Completed;
        adminLoginFrame.Margin = DeviceInfo.Idiom == DeviceIdiom.Phone ? new Thickness(0) : new Thickness(100);
        adminLoginFrame.VerticalOptions = DeviceInfo.Idiom == DeviceIdiom.Phone ? LayoutOptions.FillAndExpand : LayoutOptions.StartAndExpand;
        if (DeviceInfo.Idiom == DeviceIdiom.Phone)
        {
            verifyButton.Margin = new Thickness(0, 5, 15, 0);
            cancelButton.Margin = new Thickness(0, 5, 15, 0);
        }
        else
        {
            verifyButton.Margin = new Thickness(0, 10, 40, 0);
            cancelButton.Margin = new Thickness(0, 10, 40, 0);
        }

    }
    private void PasswordField_Completed(object? sender, EventArgs e)
    {
        verifyButton.Command.Execute(null);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _ = passwordField.Focus();
    }
}
