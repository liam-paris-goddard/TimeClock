using Goddard.Clock.ViewModels;

namespace Goddard.Clock.Factories;
public interface ILoginPageFactory
{
    LoginPage Create();
}

public class LoginPageFactory : ILoginPageFactory
{
    private readonly IServiceProvider _serviceProvider;

    public LoginPageFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [Obsolete]
    public LoginPage Create()
    {
        var viewModel = new LoginPageViewModel(_serviceProvider);
        return new LoginPage(viewModel);
    }
}
