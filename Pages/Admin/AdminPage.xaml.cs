using TimeClock.Data;
using TimeClock.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace TimeClock
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdminPage : TimedContentPage
    {
        public AdminPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            listviewEvents.ItemsSource = MyBinding.PendingClockEvents;
        }

        protected AdminPageViewModel MyBinding
        {
            get { return this.BindingContext as AdminPageViewModel; }
        }


        private async void forceSync_Clicked(object sender, EventArgs e)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("", "no network/internet connection available, resolve your connection before attempting force sync", "OK");
            }
            else
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        var msg = new ModalUserMessage("Synchronizing database...", true, true);
                        msg.Show();

                        await SyncEngine.Instance.Start();
                        await SyncEngine.Instance.ForceSend();
                        await SyncEngine.Instance.ForcePull();

                        await MyBinding.Init();

                        listviewEvents.ItemsSource = MyBinding.PendingClockEvents;

                        msg.Close();
                    }
                    catch (Exception ex)
                    {
                        TimeClock.Helpers.Logging.Log(ex);
                        throw;
                    }
                });
            }
        }
    }
}
