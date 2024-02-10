using TimeClock.Controls;
using TimeClock.Data;
using TimeClock.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TimeClock
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SchoolSelectionPage : TimedContentPage
    {
        public SchoolSelectionPage()
        {
            InitializeComponent();
        }

        protected async void PagedGoddardButtonGridButtonClick(object sender, PagedGoddardButtonGrid.ButtonClickEventArgs e)
        {
            var modalUserMessage = new ModalUserMessage("Synchronizing database...", true, true);
            modalUserMessage.Show();

            Helpers.Settings.LastSelectedSchoolID = Int64.Parse(e.SelectedValue);
            Helpers.Settings.LastSelectedSchoolName = e.SelectedText;

            await SyncEngine.Instance.Stop();

            var configTask = new RestService().GetSchoolConfiguration(Helpers.Settings.LastSelectedSchoolID);
            var startSyncTask = SyncEngine.Instance.Start();

            await Task.WhenAll(new List<Task>() { configTask, startSyncTask });

            var config = await configTask;

            if (configTask.Result != null)
            {
                Helpers.Settings.BypassSignatureEmployees = configTask.Result.BypassSignatureEmployees;
                Helpers.Settings.BypassSignatureParents = configTask.Result.BypassSignatureParents;
            }
            else
            {
                Helpers.Settings.BypassSignatureEmployees = false;
                Helpers.Settings.BypassSignatureParents = false;
            }

            modalUserMessage.Close();

            await Helpers.Navigation.ResetNavigationAndGoToRoot();
        }
    }
}
