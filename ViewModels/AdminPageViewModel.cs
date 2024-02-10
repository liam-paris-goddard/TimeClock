using TimeClock.Data;
using TimeClock.Helpers;
using TimeClock.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace TimeClock
{
    public class AdminPageViewModel : BaseViewModel
    {

        private DateTime? _latestPullDateTime;

        public DateTime? LatestPullDateTime
        {
            get { return _latestPullDateTime; }
            set
            {
                _latestPullDateTime = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _latestSendDateTime;

        public DateTime? LatestSendDateTime
        {
            get { return _latestSendDateTime; }
            set
            {
                _latestSendDateTime = value;
                OnPropertyChanged();
            }
        }

        private bool _isMultiSchoolUser = true;
        public bool IsMultiSchoolUser
        {
            get { return _isMultiSchoolUser; }
            set
            {
                _isMultiSchoolUser = value;
                OnPropertyChanged();
            }
        }

        //public ObservableCollection<EventExtended> PendingClockEvents { get; private set; } = new ObservableCollection<EventExtended>();

        private ObservableCollection<EventExtended> _pendingClockEvents = new ObservableCollection<EventExtended>();
        public ObservableCollection<EventExtended> PendingClockEvents
        {
            get { return _pendingClockEvents; }
            set
            {
                _pendingClockEvents = value;
                OnPropertyChanged();
            }
        }

        public AllowedSchool[] AllowedSchools { get; private set; }

        public class StateItem
        {
            public string Display { get; set; }
            public string Value { get; set; }
        }

        public class SchoolItem
        {
            public string Display { get; set; }
            public long Value { get; set; }
        }


        public AdminPageViewModel()
        {
            Init();

            LogoutCommand = new Command(async () =>
            {
                GlobalResources.Current.GoToMainOnPageTimeout = false;

                //TODO: leaving username set for now, probably the correct approach
                //Helpers.Settings.Username = "";
                Helpers.Settings.Password = "";
                await SyncEngine.Instance.Stop();

                await Helpers.Navigation.ResetNavigationAndGoToRoot(new LoginPage());
            });

            //ForceSyncCommand = new Command(async () =>
            //{
            //    var msg = new ModalUserMessage("Synchronizing database...", true, true);
            //    msg.Show();

            //    await SyncEngine.Instance.Start();
            //    await SyncEngine.Instance.ForceSend();
            //    await SyncEngine.Instance.ForcePull();

            //    msg.Close();

            //    Init();
            //});

            RebuildCommand = new Command(async () =>
            {
                try
                {
                    var msg = new ModalUserMessage("Rebuilding database...", true, true);
                    msg.Show();

                    await SyncEngine.Instance.Stop();
                    App.Database.RebuildDatatabase();
                    await SyncEngine.Instance.Start();

                    msg.Close();

                    await Init();
                }
                catch (Exception ex)
                {
                    TimeClock.Helpers.Logging.Log(ex);
                    throw;
                }
            });

            ChangeSchoolCommand = new Command(async () =>
            {
                GlobalResources.Current.GoToMainOnPageTimeout = false;

                var allowed = await new RestService().GetAllowedSchools(Helpers.Settings.Username);
                if (allowed == null || allowed.Count() <= 1)
                    return;
                else
                {
                    if (allowed.Count() <= 10)
                    {
                        var page = new SchoolSelectionPage();
                        ((SchoolSelectionPageViewModel)page.BindingContext).Schools = allowed.ToList();

                        await App.Current.MainPage.Navigation.PushAsync(page);
                    }
                    else
                    {
                        var page = new StateSelectionPage();
                        ((StateSelectionPageViewModel)page.BindingContext).Schools = allowed.ToList();

                        await App.Current.MainPage.Navigation.PushAsync(page);
                    }
                }
            });

        }

        public async Task Init()
        {
            IsMultiSchoolUser = Helpers.Settings.IsMultiSchoolUser;
            LatestPullDateTime = await App.Database.GetLocalSyncLogLatest(LogType.Pull);
            LatestSendDateTime = await App.Database.GetLocalSyncLogLatest(LogType.Send);

            var events = await App.Database.GetUnprocessedClockExtendedEvents(Int32.MaxValue);

            PendingClockEvents.Clear();

            foreach (var item in events)
                PendingClockEvents.Add(item);
        }

        public Command LogoutCommand { get; }
        public Command RebuildCommand { get; }
        public Command ChangeSchoolCommand { get; }
        //public Command ForceSyncCommand { get; }
    }
}
