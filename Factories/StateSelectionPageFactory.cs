using Goddard.Clock.ViewModels;
using Goddard.Clock.Models;

namespace Goddard.Clock.Factories;
public interface IStateSelectionPageFactory
{
    StateSelectionPage Create(List<AllowedSchool>? schools);
}

public class StateSelectionPageFactory : IStateSelectionPageFactory
{
    private readonly IServiceProvider _serviceProvider;

    public StateSelectionPageFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public StateSelectionPage Create(List<AllowedSchool>? schools)
    {
        var viewModel = _serviceProvider.GetRequiredService<StateSelectionPageViewModel>();
        if (schools != null)
        {
            viewModel.Schools = schools;
        }
        return new StateSelectionPage(_serviceProvider, viewModel);
    }
}