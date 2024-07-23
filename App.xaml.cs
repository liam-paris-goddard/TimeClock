using System.Diagnostics;
using System.ComponentModel;
using Goddard.Clock.Data;
using Goddard.Clock.Helpers;
using Goddard.Clock.Factories;
using Goddard.Clock.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.DataContracts;

using Goddard.Clock;
using Microsoft.Extensions.Configuration;

namespace Goddard.Clock;
public partial class App : Application, INotifyPropertyChanged
{
    private TelemetryClient _telemetryClient;
    
    private IConfiguration Configuration { get; }
    public static long? EmployeeUserPersonId { get; set; }

	private static ClockDatabase? _database;
	public static ClockDatabase? Database
	{
		get { return _database; }
		private set { _database = value; }
	}

	private static IServiceProvider? _serviceProvider;
	public static IServiceProvider? ServiceProvider
	{
		get { return _serviceProvider; }
		private set { _serviceProvider = value; }
	}

	private static NavigationService? _navigation;
	public static NavigationService? NavigationService
	{
		get { return _navigation; }
		private set { _navigation = value; }
	}

	private static SyncEngineService? _syncEngine;
	public static SyncEngineService? SyncEngine
	{
		get { return _syncEngine; }
		private set { _syncEngine = value; }
	}

	private static ViewModelLocator? _locator;
	public static ViewModelLocator? Locator
	{
		get { return _locator; }
		private set { _locator = value; }
	}


	public App(ClockDatabase database, IServiceProvider serviceProvider, TelemetryClient telemetryClient)
	{
		InitializeComponent();
        _telemetryClient = telemetryClient;
        LogAppStart();
		_database = database;
		_serviceProvider = serviceProvider;
		_locator = _serviceProvider.GetRequiredService<ViewModelLocator>();
		_navigation = _serviceProvider.GetRequiredService<NavigationService>();
		_syncEngine = _serviceProvider.GetRequiredService<SyncEngineService>();
		var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
		updateDeviceInfo(mainDisplayInfo.Width, mainDisplayInfo.Height, mainDisplayInfo.Orientation);
		setDeviceType(mainDisplayInfo);
		// Handle the MainDisplayInfoChanged event
		DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
		try
		{
			GlobalResources.Current.UpdateCurrentDateTime();
			GlobalResources.Current.UpdateLastUserInteraction();
			IDispatcherTimer logoutTimer = Dispatcher.CreateTimer();
			logoutTimer.Interval = TimeSpan.FromSeconds(60);
			logoutTimer.Tick += (sender, e) =>
				{
					GlobalResources.Current.UpdateCurrentDateTime();

					var isNormalRunMode =
						Current?.MainPage != null &&
						Current.MainPage.Navigation.NavigationStack.FirstOrDefault() is HomePage;

					/*if (GlobalResources.Current.HasPageTimedOut && isNormalRunMode)
					{
						//any time they let it idle timeout, it should assume that we leave employee/child mode
						App.EmployeeUserPersonId = null;

						_navigation.ResetNavigationAndGoToRoot();
					}*/
				};


			//so this will check for updates on start/restart and then once every day thereafter
			IDispatcherTimer updateTimer = Dispatcher.CreateTimer();
			updateTimer.Interval = TimeSpan.FromSeconds(10);
			updateTimer.Tick += (sender, e) =>
			{
				CheckUpdates();
                updateTimer.Interval = TimeSpan.FromHours(24);
			};

			updateTimer.Start();
			logoutTimer.Start();

			if (!Settings.HasCredentials() || Settings.LastSelectedSchoolID == 0) //user never logged in or logged in and did not select school
			{
				var factory = _serviceProvider.GetRequiredService<ILoginPageFactory>();
				var page = factory.Create();
				MainPage = new NavigationPage(page);
			}
			else if (Settings.LastSelectedSchoolID <= 0) //user has logged in before but no last school saved
			{
				Settings.BypassSignatureEmployees = false;
				Settings.BypassSignatureParents = false;
				var factory = _serviceProvider.GetRequiredService<IStateSelectionPageFactory>();
				var page = factory.Create(new List<AllowedSchool>());
				MainPage = new NavigationPage(page);
			}
			else //we've got all the info we need to go straight into the app
			{
				var factory = _serviceProvider.GetRequiredService<IHomePageFactory>();
				var page = factory.Create();
				MainPage = new NavigationPage(page);
			}

			_syncEngine.PullRemoteFinished += Instance_PullRemoteFinished;
			_syncEngine.PullRemoteStarted += Instance_PullRemoteStarted;
			_syncEngine.SendRemoteStarted += Instance_SendRemoteStarted;
			_syncEngine.SendRemoteFinished += Instance_SendRemoteFinished;
		}
		catch (Exception ex)
		{
			_ = Logging.Log(_database, ex);
			throw;
		}
	}
	void setDeviceType(DisplayInfo displayInfo)
	{
		var deviceString = "";
		var fieldToCheck = displayInfo.Orientation == DisplayOrientation.Portrait ? displayInfo.Height : displayInfo.Width;
		if (fieldToCheck <= 2266) // TODO width less than 2224
		{
			deviceString = "small";
		}
		else if (fieldToCheck < 2732) // TODO width less than 2360
		{
			deviceString = "medium";
		}
		else
		{
			deviceString = "large";
		}
		DeviceInformation.Instance.DeviceType = deviceString;
	}
	void updateDeviceInfo(double width, double height, DisplayOrientation orientation)
	{
		// Update your global variables
		DeviceInformation.Instance.GlobalOrientation = orientation;
		DeviceInformation.Instance.Width = width;
		DeviceInformation.Instance.Height = height;
		var deviceName = ConstantsStatics.iOSDeviceModels.FirstOrDefault(x =>
		{
			if (DeviceInformation.Instance.GlobalOrientation == DisplayOrientation.Portrait)
			{
				return width == x.Value.Width && height == x.Value.Height;
			}
			else
			{
				return width == x.Value.Height && height == x.Value.Width;
			}
		}).Key;
		if (!string.IsNullOrEmpty(deviceName))
		{
			DeviceInformation.Instance.DisplayInformation = ConstantsStatics.iOSDeviceModels[deviceName];
		}
		else
		{
			DeviceInformation.Instance.DisplayInformation = ConstantsStatics.iOSDeviceModels["base"];
		}
	}
	void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
	{
		updateDeviceInfo(e.DisplayInfo.Width, e.DisplayInfo.Height, e.DisplayInfo.Orientation);
	}

	protected static async void CheckUpdates()
	{
		try
		{
			var updateHelper = _serviceProvider!.GetRequiredService<IAutoUpdateHelper>();
			if (updateHelper != null)
			{
				if (await updateHelper.IsUpdateAvailableAsync())
				{
					updateHelper.DownloadInstallUpdate();
				}
			}
			else
			{
				await Logging.Log(_database, "Update Helper not found by Dependency Service");
			}
		}
		catch (Exception ex)
		{
			Logging.DebugWrite(ex);
			await Logging.Log(_database, ex);
		}
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		Window window = base.CreateWindow(activationState);
		window.Created += (sender, e) =>
		{
			if (DeviceInfo.Current != null)
			{
				Settings.DeviceID = DeviceInfo.Current.Idiom.ToString();
				var autoUpdateHelper = _serviceProvider!.GetRequiredService<IAutoUpdateHelper>();
				var verisonNumber = "no version number found";
				if (autoUpdateHelper != null)
				{
					verisonNumber = autoUpdateHelper.GetLocalVersionNumber();
				}

				var description = String.Format("{0} {1} {2} {4} - App Version: {3}", DeviceInfo.Current.Model,
					DeviceInfo.Current.Platform, DeviceInfo.Current.Version, verisonNumber, DeviceInfo.Current.Idiom);
				Settings.DeviceDescription = description;
			}
			else
			{
				_ = Logging.Log(_database, "DeviceInfo.Current is null in App.xaml.cs OnStart()");
			}
		};

		window.Resumed += async (sender, args) =>
		{
			if (_syncEngine != null)
				await _syncEngine.Start();
			DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;

		};

		window.Deactivated += async (sender, args) =>
		{
			if (_syncEngine != null)
				await _syncEngine.Stop();
			DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;		};

		return window;

	}

	//note that these are defined as static to allow them to work even if the instance
	//that sets them up goes out of scope
	public static void Instance_SendRemoteFinished(object? sender, EventArgs e)
	{
		Debug.WriteLine("Instance_SendRemoteFinished event handler invoked");
	}

	public static void Instance_SendRemoteStarted(object? sender, EventArgs e)
	{
		Debug.WriteLine("Instance_SendRemoteStarted event handler invoked");
	}

	public static void Instance_PullRemoteStarted(object? sender, EventArgs e)
	{
		Debug.WriteLine("Instance_PullRemoteStarted event handler invoked");
	}

	public static void Instance_PullRemoteFinished(object? sender, EventArgs e)
	{
		Debug.WriteLine("Instance_PullRemoteFinished event handler invoked");
	}

            private void LogAppStart()
        {
            _telemetryClient.TrackEvent("AppStarted");
            _telemetryClient.Flush();
        }

        protected override void OnStart()
        {
            base.OnStart();
            _telemetryClient.TrackEvent("AppOnStart");
            _telemetryClient.Flush();
        }
}
