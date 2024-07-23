using Goddard.Clock.Data;
using Goddard.Clock.Models;
using System.Diagnostics;

namespace Goddard.Clock.Helpers;

public class SyncEngineService(IServiceProvider serviceProvider)
{
    public bool IsStarted { get; private set; }
    private readonly ClockDatabase? _database = serviceProvider.GetService<ClockDatabase>();

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
        //TODO: do we want to do something more advanced here?
        Debug.WriteLine("SyncEngine " + type.ToString().ToUpper() + ": " + message);
        OnSyncEngineLog(new SyncEngineLogEventArgs(message, type, DateTime.Now));
    }

    private bool HasRequiredSettings()
    {
        return
            Settings.LastSelectedSchoolID > 0 &&
            !String.IsNullOrWhiteSpace(Settings.Username) &&
            !String.IsNullOrWhiteSpace(Settings.Password);
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
            return await SendLocalData();
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
        try
        {
            using (await _instanceLock.LockAsync())
            {
                if (!IsStarted)
                {
                    if (!HasRequiredSettings())
                        return;

                    IsStarted = true;

                    //always start with a pull and send
                    _ = await _PullRemoteData();
                    _ = await SendLocalData();
                    _ = await _PerformMaintenance();


                    if(Application.Current?.Dispatcher != null)
                    {
                        IDispatcherTimer sendTimer = Application.Current.Dispatcher.CreateTimer();
                        sendTimer.Interval = TimeSpan.FromMilliseconds(_SendInterval);
                        sendTimer.Tick += async (sender, e) =>
                        {
                            Log(SyncEngineLogType.Info, "Send Timer Fired");
                            _ = await SendLocalData();
                        };

                        IDispatcherTimer pullTimer = Application.Current.Dispatcher.CreateTimer();
                        pullTimer.Interval = TimeSpan.FromMilliseconds(_PullInterval);
                        pullTimer.Tick += async (sender, e) =>
                        {
                            Log(SyncEngineLogType.Info, "Pull Timer Fired");
                            _ = await _PullRemoteData();
                        };

                        IDispatcherTimer maintenanceTimer = Application.Current.Dispatcher.CreateTimer();
                        maintenanceTimer.Interval = TimeSpan.FromMilliseconds(_MaintenanceInterval);
                        maintenanceTimer.Tick += async (sender, e) =>
                        {
                            Log(SyncEngineLogType.Info, "Maintenance Timer Fired");
                            _ = await _PerformMaintenance();
                        };

                        sendTimer.Start();
                        pullTimer.Start();
                        maintenanceTimer.Start();
                    }

                    Connectivity.ConnectivityChanged += Current_ConnectivityChanged;
                }
            }
        }
        catch (Exception ex)
        {
            await Logging.Log(_database, ex);
        }
    }

    private async void Current_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs args)
    {
        if (args.NetworkAccess == NetworkAccess.Internet)
        {
            await Start();
        }
        else
        {
            await Stop();
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
    private static AsyncLock _databaseLock = new();
    private static AsyncLock _instanceLock = new();


    private async Task<bool> _PerformMaintenance()
    {
        try
        {
            using (await _databaseLock.LockAsync())
            {
                Log(SyncEngineLogType.Info, "Entered _PerformMaintenance");
                OnMaintenanceStarted(new EventArgs());
                var myStopwatch = Stopwatch.StartNew();

                if(_database != null){_ = await _database.PurgeClockEvent(_PURGE_WINDOW_DAYS);

                _ = await _database.PurgeUpdatePIN(_PURGE_WINDOW_DAYS);

                _ = await _database.PurgeLocalSyncLog();

                _ = await _database.PurgeLocalLog(_PURGE_WINDOW_DAYS);

                _ = await _database.CompactDatabase();}

                //TODO: give alert to server for any records that are old but never processed?  can that even happen in theory?
                //seems like the only way it could happen is if the clock has been offline for more than the window
                //and then we're not going to get the alert anyway, so maybe if anything, it should show some kind of 
                //blocking alert on the device itself to alert the local staff?


                Log(SyncEngineLogType.Info, String.Format("Leaving _PerformMaintenance:  {0}ms", myStopwatch.ElapsedMilliseconds));
                OnMaintenanceFinished(new EventArgs());
                return true;
            }
        }
        catch (Exception ex)
        {
            await Logging.Log(_database, ex);
            return false;
        }
    }

    //TODO: while there is every reason to believe that this code is fundamentally correct
    //the truth is that at the time of writing there is no way to generate substantial amounts
    //of data for any meaningful level of testing, so this should definitely be rechecked
    //carefully once that has changed
    private async Task<bool> SendLocalData()
    {
                        var result = false;
        try
        {
            using (await _databaseLock.LockAsync())
            {
                Log(SyncEngineLogType.Info, "Entered SendLocalData");
                OnSendRemoteStarted(new EventArgs());
                var myStopwatch = Stopwatch.StartNew();


                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    //TODO: address scenario where if web api throws exception this will just continually
                    //loop retrying bad data input
                    if (_database != null)
                    {
                        var pins = await _database.GetUnprocessedUpdatePINs(_WEBSERVICE_BATCH_SIZE);
                        var events = await _database.GetUnprocessedClockEvents(_WEBSERVICE_BATCH_SIZE);
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
                                _ = await _database.SetLocalEntitiesUploaded(allEntities, DateTime.Now);
                                return true;
                            }

                            pins = await _database.GetUnprocessedUpdatePINs(_WEBSERVICE_BATCH_SIZE);
                            events = await _database.GetUnprocessedClockEvents(_WEBSERVICE_BATCH_SIZE);
                            loop_counter++;
                        }


                        var logs = await _database.GetUnprocessedLogs(_WEBSERVICE_BATCH_SIZE);
                        loop_counter = 0;
                        while (logs.Count > 0 && loop_counter < _MaximumSendLoopCount)
                        {
                            var upload = await new RestService().SendLogs(logs.ToArray());
                            if (upload)
                            {
                                _ = await _database.SetLocalEntitiesUploaded(logs, DateTime.Now);
                                return true;
                            }

                            logs = await _database.GetUnprocessedLogs(_WEBSERVICE_BATCH_SIZE);
                            loop_counter++;
                        }

                        _ = await _database.RegisterSend();
                    }
                    else
                    {
                        Log(SyncEngineLogType.Info, "SendLocalData Network Unvailable");
                    }

                    Log(SyncEngineLogType.Info, String.Format("Leaving SendLocalData: {0}ms", myStopwatch.ElapsedMilliseconds));
                    OnSendRemoteFinished(new EventArgs());
                    return result;
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            await Logging.Log(_database, ex);
            return false;
        }
    }

    private async Task<bool> _PullRemoteData()
    {
        try
        {
            using (await _databaseLock.LockAsync())
            {
                Log(SyncEngineLogType.Info, "Entered _PullRemoteData");
                OnPullRemoteStarted(new EventArgs()); await Task.Delay(5000);

                var myStopwatch = Stopwatch.StartNew();

                var result = false;
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    var data = await new RestService().GetSyncData(Settings.LastSelectedSchoolID);


                    if (_database != null)
                        result = await _database.SyncRemoteData(data);

                    //this will have the server log that the pull successfully completed
                    if (result)
                    {
                        _ = await new RestService().RegisterPullCompleted();
                        if (_database != null)

                            _ = await _database.RegisterPull();
                    }
                }
                else
                {
                    Log(SyncEngineLogType.Info, "_PullRemoteData Network Unvailable");
                }

                Log(SyncEngineLogType.Info, String.Format("Leaving _PullRemoteData: {0}ms", myStopwatch.ElapsedMilliseconds));

                OnPullRemoteFinished(new EventArgs());
                return result;
            }
        }
        catch (Exception ex)
        {
            await Logging.Log(_database, ex);
            return false;
        }
    }

    //TODO: this https://developer.xamarin.com/guides/xamarin-forms/messaging-center/
    //may be a "better" way to handle this since it seems more geared towards multiple
    //totally de-coupled things to be made aware of events

    //TODO: the application should subscribe to these two events and do something
    //to stall/block user action during the pulldown since the user interacting with
    //the data during that could cause problems, one basic idea would be to put up
    //a blocking wait spinner for Started and remove it for Finished
    public event EventHandler? PullRemoteStarted;
    public event EventHandler? PullRemoteFinished;

    private void OnPullRemoteStarted(EventArgs e)
    {
        PullRemoteStarted?.Invoke(this, e);
    }
    private void OnPullRemoteFinished(EventArgs e)
    {
        PullRemoteFinished?.Invoke(this, e);
    }

    //TODO: sending is probably not going to require any blocking of the user
    //so these events are mainly provided for the sake of OCD or if we want to implement
    //some kind of logging
    public event EventHandler? SendRemoteStarted;
    public event EventHandler? SendRemoteFinished;
    private void OnSendRemoteStarted(EventArgs e)
    {
        SendRemoteStarted?.Invoke(this, e);
    }
    private void OnSendRemoteFinished(EventArgs e)
    {
        SendRemoteFinished?.Invoke(this, e);
    }

    //TODO: again for sake of OCD
    public event EventHandler? MaintenanceStarted;
    public event EventHandler? MaintenanceFinished;
    private void OnMaintenanceStarted(EventArgs e)
    {
        MaintenanceStarted?.Invoke(this, e);
    }
    private void OnMaintenanceFinished(EventArgs e)
    {
        MaintenanceFinished?.Invoke(this, e);
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

    public delegate void SyncEngineLogEventHandler(object? sender, SyncEngineLogEventArgs e);

    //TODO: whatever the main app code uses for logging should probably subscribe to this event
    public event SyncEngineLogEventHandler? SyncEngineLog;
    private void OnSyncEngineLog(SyncEngineLogEventArgs e)
    {
        SyncEngineLog?.Invoke(this, e);
    }

}