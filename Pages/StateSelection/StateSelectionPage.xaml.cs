using Goddard.Clock.Controls;
using Goddard.Clock.Factories;
using Goddard.Clock.ViewModels;

namespace Goddard.Clock;
public partial class StateSelectionPage : TimedContentPage
{
    private readonly IServiceProvider _serviceProvider;
    public StateSelectionPage(IServiceProvider serviceProvider, StateSelectionPageViewModel viewModel)
    {
        _serviceProvider = serviceProvider;
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected void PagedGoddardButtonGridButtonClick(object? sender, PagedGoddardButtonGrid.ButtonClickEventArgs e)
    {
        if (e != null && e.SelectedValue != null)
        {
            var schoolsForSelectedState = ((StateSelectionPageViewModel)BindingContext).GetSchoolsByState(e.SelectedValue).ToList();

            var factory = _serviceProvider.GetRequiredService<ISchoolSelectionPageFactory>();
            var page = factory.Create(schoolsForSelectedState);

            _ = Navigation.PushAsync(page, false);
        }
    }
}
