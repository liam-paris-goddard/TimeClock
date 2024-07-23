using Goddard.Clock.ViewModels;

namespace Goddard.Clock.Factories;
public interface IAdminLoginPageFactory
{
    AdminLoginPage Create();
}

public class AdminLoginPageFactory : IAdminLoginPageFactory
{
    private readonly IServiceProvider _serviceProvider;

    public AdminLoginPageFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [Obsolete]
    public AdminLoginPage Create()
    {

        var viewModel = new AdminLoginPageViewModel(_serviceProvider);
        return new AdminLoginPage(viewModel);
    }
}
