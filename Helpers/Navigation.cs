using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace TimeClock.Helpers
{
    public static class Navigation
    {
        public static async Task ResetNavigationAndGoToRoot(Page rootPage = null)
        {
            try
            {
                if (rootPage == null)
                    rootPage = new HomePage();

                var navigationPage = (NavigationPage)Application.Current.Application.MainPage;
                navigationPage.Navigation.InsertPageBefore(rootPage, navigationPage.Navigation.NavigationStack.First());

                await navigationPage.Navigation.PopToRootAsync(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Reset Nav Error: " + ex.ToString());
            }
        }
    }
}

/**
TODO
Please note that you'll need to replace HomePage with the actual root page of your .NET MAUI application. Also, ensure that your HomePage is a Page type, not a View type.
*/