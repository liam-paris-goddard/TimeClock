using Goddard.Clock.ViewModels;

namespace Goddard.Clock.Factories;
public interface IEmployeeSelectionPageFactory
{
    EmployeeSelectionPage Create();
}

public class EmployeeSelectionPageFactory : IEmployeeSelectionPageFactory
{
    private readonly IServiceProvider _serviceProvider;

    public EmployeeSelectionPageFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public EmployeeSelectionPage Create()
    {
        var viewModel = new EmployeeSelectionPageViewModel();
        return new EmployeeSelectionPage(_serviceProvider, viewModel);
    }
}