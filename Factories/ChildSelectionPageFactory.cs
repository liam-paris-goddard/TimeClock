using Goddard.Clock.ViewModels;
using Goddard.Clock.Models;

namespace Goddard.Clock.Factories;
public interface IChildSelectionPageFactory
{
    ChildSelectionPage Create(List<Child>? children, long? employeeUserPersonId);
}

public class ChildSelectionPageFactory : IChildSelectionPageFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ChildSelectionPageFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ChildSelectionPage Create(List<Child>? children, long? employeeUserPersonId)
    {
        var viewModel = new ChildSelectionPageViewModel();
        if (children != null)
            viewModel.Children = children;
        if (employeeUserPersonId != null)
            viewModel.EmployeeUserPersonId = App.EmployeeUserPersonId;

        return new ChildSelectionPage(_serviceProvider, viewModel);
    }
}