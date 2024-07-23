using System.Diagnostics;
using Goddard.Clock.Factories;
namespace Goddard.Clock.Helpers;
public class NavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public bool isFromModal = false;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ResetNavigationAndGoToRoot(Page? root = null)
    {
        try
        {
            var rootPage = root;
            if (root == null)
            {
                var homePageFactory = _serviceProvider.GetRequiredService<IHomePageFactory>();
                rootPage = homePageFactory.Create();
            }

            if (Application.Current?.MainPage is NavigationPage navigationPage)
            {
                navigationPage.Navigation.InsertPageBefore(rootPage, navigationPage.Navigation.NavigationStack[0]);
                await navigationPage.Navigation.PopToRootAsync(false);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Reset Nav Error: {ex}");
            // Consider logging the error or showing a user-friendly message
        }
    }
}

