using Goddard.Clock.ViewModels;
using Goddard.Clock.Models;
using Microsoft.ApplicationInsights;
namespace Goddard.Clock.Factories;
public interface ISchoolSelectionPageFactory
{
    SchoolSelectionPage Create(List<AllowedSchool> schools);
}

public class SchoolSelectionPageFactory : ISchoolSelectionPageFactory
{
    private readonly IServiceProvider _serviceProvider;

    public SchoolSelectionPageFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public SchoolSelectionPage Create(List<AllowedSchool> schools)
    {
        var viewModel = _serviceProvider.GetRequiredService<SchoolSelectionPageViewModel>();
        viewModel.Schools = schools;
        var telemetryClient = _serviceProvider.GetRequiredService<TelemetryClient>();
        return new SchoolSelectionPage(viewModel, telemetryClient);
    }
}
