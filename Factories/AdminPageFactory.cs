using Goddard.Clock.ViewModels;

namespace Goddard.Clock.Factories;
public interface IAdminPageFactory
{
    AdminPage Create();
}

public class AdminPageFactory : IAdminPageFactory
{
    private readonly IServiceProvider _serviceProvider;

    public AdminPageFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public AdminPage Create()
    {
        var viewModel = new AdminPageViewModel(_serviceProvider);
        return new AdminPage(viewModel);
    }
}