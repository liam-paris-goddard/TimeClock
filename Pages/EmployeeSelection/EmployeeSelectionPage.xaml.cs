using Goddard.Clock.Controls;
using Goddard.Clock.Models;
using Goddard.Clock.Factories;
using Goddard.Clock.ViewModels;
namespace Goddard.Clock;
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class EmployeeSelectionPage : TimedContentPage
{
    private readonly IServiceProvider _serviceProvider;
    public EmployeeSelectionPage(IServiceProvider serviceProvider, EmployeeSelectionPageViewModel viewModel)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected void PagedGoddardButtonGridButtonClick(object? sender, PagedGoddardButtonGrid.ButtonClickEventArgs e)
    {
        long personId = 0;

        if (!Int64.TryParse(e.SelectedValue, out personId))
        {
            //TODO: error.  or log.  or something.
        }

        var factory = _serviceProvider.GetRequiredService<IPinPadPageFactory>();
        var page = factory.Create(UserType.Employee, e.SelectedText, personId);
        _ = Navigation.PushAsync(page,
            false);

    }
}
