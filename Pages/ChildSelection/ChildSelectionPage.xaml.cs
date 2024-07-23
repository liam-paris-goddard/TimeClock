using Goddard.Clock.Controls;
using Goddard.Clock.Models;
using Goddard.Clock.Factories;
using Goddard.Clock.ViewModels;
namespace Goddard.Clock;
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ChildSelectionPage : TimedContentPage
{
    private readonly IServiceProvider _serviceProvider;
    public ChildSelectionPage(IServiceProvider serviceProvider, ChildSelectionPageViewModel viewModel)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected void PagedGoddardButtonGridButtonClick(object? sender, PagedGoddardButtonGrid.ButtonClickEventArgs e)
    {
        long familyId = 0;

        if (!Int64.TryParse(e.SelectedValue, out familyId))
        {
            //TODO: error.  or log.  or something.
        }

        var viewModel = (ChildSelectionPageViewModel)BindingContext;

        if (!viewModel.EmployeeUserPersonId.HasValue)
        {
            //TODO: error/log
        }
        else
        {

            var factory = _serviceProvider.GetRequiredService<IPreCheckInPageFactory>();
            
            var page = factory.Create(UserType.Employee, viewModel.EmployeeUserPersonId.Value, viewModel.EmployeeUserFN, viewModel.EmployeeUserLN, familyId);
            _ = Navigation.PushAsync(page,
                false);
        }
    }

    private void BackButtonClick(object? sender, EventArgs e) {
            var factory = _serviceProvider.GetRequiredService<IHomePageFactory>();
            var page = factory.Create(false);
            _ = Navigation.PushAsync(page, false);
    }
}
