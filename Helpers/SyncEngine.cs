using TimeClock.Data;
using TimeClock.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Net.NetworkInformation;
using Microsoft.Maui.Networking;
namespace TimeClock.Helpers
{
    //implemented with singleton pattern
    public sealed class SyncEngine
    {
        #region singleton pattery witchery
        private static readonly SyncEngine instance = new SyncEngine();

        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        static SyncEngine() { }

        private SyncEngine() { }

        public static SyncEngine Instance { get { return instance; } }
        #endregion

        public bool IsStarted { get; private set; }

        //private static readonly int _PullInterval = 2 * 60 * 60 * 1000; //two hours
        private static readonly int _PullInterval = 30 * 60 * 1000; //30 minutes
        private static readonly int _SendInterval = 5 * 60 * 1000; //5 minutes    
        private static readonly int _MaintenanceInterval = 24 * 60 * 60 * 1000; //one day        

        private static readonly int _MaximumSendLoopCount = 10;

        public enum SyncEngineLogType
        {
            Exception = 0,
            FatalException = 1,
            Info = 2,
            Debug = 3
        }

        public void Log(SyncEngineLogType type, string message)
        {
            //xTODO: do we want to do something more advanced here?
            Debug.WriteLine("SyncEngine " + type.ToString().ToUpper() + ": " + message);
            OnSyncEngineLog(new SyncEngineLogEventArgs(message, type, DateTime.Now));
        }

        private bool _HasRequiredSettings()
        {
            return
                Helpers.Settings.LastSelectedSchoolID > 0 &&
                !String.IsNullOrWhiteSpace(Helpers.Settings.Username) &&
                !String.IsNullOrWhiteSpace(Helpers.Settings.Password);
        }

        public async Task<bool> ForcePull()
        {
            using (await _instanceLock.LockAsync())
            {
                return await _PullRemoteData();
            }
        }

        public async Task<bool> ForceSend()
        {
            using (await _instanceLock.LockAsync())
            {
                return await _SendLocalData();
            }
        }

        public async Task<bool> ForceMaintenance()
        {
            using (await _instanceLock.LockAsync())
            {
                return await _PerformMaintenance();
            }
        }

        public async Task Start()
        {
            using (await _instanceLock.LockAsync())
            {
                if (!IsStarted)
                {
                    if (!_HasRequiredSettings())
                        return;

                    IsStarted = true;

                    //always start with a pull and send
                    await _PullRemoteData();
                    await _SendLocalData();
                    await _PerformMaintenance();

                    Device.StartTimer(TimeSpan.FromMilliseconds(_SendInterval), () =>
                    {
                        this.Log(SyncEngineLogType.Info, "Send Timer Fired");
                        _SendLocalData();
                        return IsStarted;
                    });

                    Device.StartTimer(TimeSpan.FromMilliseconds(_PullInterval), () =>
                    {
                        this.Log(SyncEngineLogType.Info, "Pull Timer Fired");
                        _PullRemoteData();
                        return IsStarted;
                    });

                    Device.StartTimer(TimeSpan.FromMilliseconds(_MaintenanceInterval), () =>
                    {
                        this.Log(SyncEngineLogType.Info, "Maintenance Timer Fired");
                        _PerformMaintenance();
                        return IsStarted;
                    });

                    Connectivity.ConnectivityChanged += Current_ConnectivityChanged;
                }
            }
        }

        private void Current_ConnectivityChanged(object sender, ConnectivityChangedEventArgs args)
        {
            if (args.IsConnected)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        public async Task Stop()
        {
            using (await _instanceLock.LockAsync())
            {
                IsStarted = false;
                Connectivity.ConnectivityChanged -= Current_ConnectivityChanged;
            }
        }

        //pure processed records older than this number days
        private const int _PURGE_WINDOW_DAYS = 7;
        //the web service supports sending update calls in batches, technically they could be of any size
        //but want to throttle so that any single call isn't a monolith
        private const int _WEBSERVICE_BATCH_SIZE = 10;

        //each of the batch operations of this engine are controlled by this such that only
        //one can be active at any given moment, while it's like some could co-run this ensures
        //that there won't be concurrency problems
        private static AsyncLock _databaseLock = new AsyncLock();
        private static AsyncLock _instanceLock = new AsyncLock();


        private async Task<bool> _PerformMaintenance()
        {
            try
            {
                using (await _databaseLock.LockAsync())
                {
                    this.Log(SyncEngineLogType.Info, "Entered _PerformMaintenance");
                    OnMaintenanceStarted(new EventArgs());
                    var myStopwatch = Stopwatch.StartNew();

                    await App.Database.PurgeClockEvent(_PURGE_WINDOW_DAYS);

                    await App.Database.PurgeUpdatePIN(_PURGE_WINDOW_DAYS);

                    await App.Database.PurgeLocalSyncLog();

                    await App.Database.PurgeLocalLog(_PURGE_WINDOW_DAYS);

                    await App.Database.CompactDatabase();

                    //xTODO: give alert to server for any records that are old but never processed?  can that even happen in theory?
                    //seems like the only way it could happen is if the clock has been offline for more than the window
                    //and then we're not going to get the alert anyway, so maybe if anything, it should show some kind of 
                    //blocking alert on the device itself to alert the local staff?


                    this.Log(SyncEngineLogType.Info, String.Format("Leaving _PerformMaintenance:  {0}ms", myStopwatch.ElapsedMilliseconds));
                    OnMaintenanceFinished(new EventArgs());
                    return true;
                }
            }
            catch (Exception ex)
            {
                TimeClock.Helpers.Logging.Log(ex);
                return false;
            }
        }

        //xTODO: while there is every reason to believe that this code is fundamentally correct
        //the truth is that at the time of writing there is no way to generate substantial amounts
        //of data for any meaningful level of testing, so this should definitely be rechecked
        //carefully once that has changed
        private async Task<bool> _SendLocalData()
        {
            try
            {
                using (await _databaseLock.LockAsync())
                {
                    this.Log(SyncEngineLogType.Info, "Entered _SendLocalData");
                    OnSendRemoteStarted(new EventArgs());
                    var myStopwatch = Stopwatch.StartNew();

                    var result = false;
                    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        //xTODO: address scenario where if web api throws exception this will just continually
                        //loop retrying bad data input
                        var pins = await App.Database.GetUnprocessedUpdatePINs(_WEBSERVICE_BATCH_SIZE);
                        var events = await App.Database.GetUnprocessedClockEvents(_WEBSERVICE_BATCH_SIZE);
                        var loop_counter = 0;
                        while ((pins.Count > 0 || events.Count > 0) && loop_counter < _MaximumSendLoopCount)
                        {
                            var myData = new SendSyncData(events.ToArray(), pins.ToArray());
                            var upload = await new RestService().SendSyncData(myData);
                            if (upload)
                            {
                                var allEntities = new List<LocalEntity>();
                                allEntities.AddRange(myData.Events.ToList<LocalEntity>());
                                allEntities.AddRange(myData.UpdatePINs.ToList<LocalEntity>());
                                await App.Database.SetLocalEntitiesUploaded(allEntities, DateTime.Now);
                            }

                            pins = await App.Database.GetUnprocessedUpdatePINs(_WEBSERVICE_BATCH_SIZE);
                            events = await App.Database.GetUnprocessedClockEvents(_WEBSERVICE_BATCH_SIZE);
                            loop_counter++;
                        }


                        var logs = await App.Database.GetUnprocessedLogs(_WEBSERVICE_BATCH_SIZE);
                        loop_counter = 0;
                        while (logs.Count > 0 && loop_counter < _MaximumSendLoopCount)
                        {
                            var upload = await new RestService().SendLogs(logs.ToArray());
                            if (upload)
                            {
                                await App.Database.SetLocalEntitiesUploaded(logs, DateTime.Now);
                            }

                            logs = await App.Database.GetUnprocessedLogs(_WEBSERVICE_BATCH_SIZE);
                            loop_counter++;
                        }

                        await App.Database.RegisterSend();
                    }
                    else
                    {
                        this.Log(SyncEngineLogType.Info, "_SendLocalData Network Unvailable");
                    }

                    this.Log(SyncEngineLogType.Info, String.Format("Leaving _SendLocalData: {0}ms", myStopwatch.ElapsedMilliseconds));
                    OnSendRemoteFinished(new EventArgs());
                    return result;
                }
            }
            catch (Exception ex)
            {
                TimeClock.Helpers.Logging.Log(ex);
                return false;
            }
        }

        private async Task<bool> _PullRemoteData()
        {
            try
            {
                using (await _databaseLock.LockAsync())
                {
                    this.Log(SyncEngineLogType.Info, "Entered _PullRemoteData");
                    OnPullRemoteStarted(new EventArgs()); await Task.Delay(5000);

                    var myStopwatch = Stopwatch.StartNew();

                    var result = false;
                    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        var data = await new RestService().GetSyncData(Helpers.Settings.LastSelectedSchoolID);

                        result = await App.Database.SyncRemoteData(data);

                        //this will have the server log that the pull successfully completed
                        if (result)
                        {
                            await new RestService().RegisterPullCompleted();
                            await App.Database.RegisterPull();
                        }
                    }
                    else
                    {
                        this.Log(SyncEngineLogType.Info, "_PullRemoteData Network Unvailable");
                    }

                    this.Log(SyncEngineLogType.Info, String.Format("Leaving _PullRemoteData: {0}ms", myStopwatch.ElapsedMilliseconds));

                    OnPullRemoteFinished(new EventArgs());
                    return result;
                }
            }
            catch (Exception ex)
            {
                TimeClock.Helpers.Logging.Log(ex);
                return false;
            }
        }

        //xTODO: this https://developer.xamarin.com/guides/xamarin-forms/messaging-center/
        //may be a "better" way to handle this since it seems more geared towards multiple
        //totally de-coupled things to be made aware of events

        //xTODO: the application should subscribe to these two events and do something
        //to stall/block user action during the pulldown since the user interacting with
        //the data during that could cause problems, one basic idea would be to put up
        //a blocking wait spinner for Started and remove it for Finished
        public event EventHandler PullRemoteStarted;
        public event EventHandler PullRemoteFinished;

        private void OnPullRemoteStarted(EventArgs e)
        {
            if (PullRemoteStarted != null)
                PullRemoteStarted(this, e);
        }
        private void OnPullRemoteFinished(EventArgs e)
        {
            if (PullRemoteFinished != null)
                PullRemoteFinished(this, e);
        }

        //xTODO: sending is probably not going to require any blocking of the user
        //so these events are mainly provided for the sake of OCD or if we want to implement
        //some kind of logging
        public event EventHandler SendRemoteStarted;
        public event EventHandler SendRemoteFinished;
        private void OnSendRemoteStarted(EventArgs e)
        {
            if (SendRemoteStarted != null)
                SendRemoteStarted(this, e);
        }
        private void OnSendRemoteFinished(EventArgs e)
        {
            if (SendRemoteFinished != null)
                SendRemoteFinished(this, e);
        }

        //xTODO: again for sake of OCD
        public event EventHandler MaintenanceStarted;
        public event EventHandler MaintenanceFinished;
        private void OnMaintenanceStarted(EventArgs e)
        {
            if (MaintenanceStarted != null)
                MaintenanceStarted(this, e);
        }
        private void OnMaintenanceFinished(EventArgs e)
        {
            if (MaintenanceFinished != null)
                MaintenanceFinished(this, e);
        }


        public class SyncEngineLogEventArgs : EventArgs
        {
            public SyncEngineLogType Type { get; set; }
            public string Message { get; set; }
            public DateTime Occurred { get; set; }

            public SyncEngineLogEventArgs(string message, SyncEngineLogType type, DateTime occurred)
            {
                Message = message;
                Occurred = occurred;
                Type = type;
            }

            public SyncEngineLogEventArgs(string message, SyncEngineLogType type)
                : this(message, type, DateTime.Now)
            {

            }
        }

        public delegate void SyncEngineLogEventHandler(Object sender, SyncEngineLogEventArgs e);

        //xTODO: whatever the main app code uses for logging should probably subscribe to this event
        public event SyncEngineLogEventHandler SyncEngineLog;
        private void OnSyncEngineLog(SyncEngineLogEventArgs e)
        {
            if (SyncEngineLog != null)
                SyncEngineLog(this, e);
        }

    }
}