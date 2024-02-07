using TimeClock.Data;
using TimeClock.Helpers;
using TimeClock.Models;
using Microsoft.Maui.Controls; // Replaces Xamarin.Forms
using Microsoft.Maui; // Replaces Plugin.DeviceInfo and Plugin.Settings
using Microsoft.Extensions.DependencyInjection; // Replaces Microsoft.Practices.Unity
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeClock
{
	public partial class App : Application
	{
		private static ClockDatabase database;
		public static ClockDatabase Database
		{
			get
			{
				if (database == null)
				{
					var path = DependencyService.Get<IFileHelper>().GetLocalFilePath("GoddardClock.db3");
					Debug.WriteLine(path);
					database = new ClockDatabase(path);
				}
				return database;
			}
		}

		//making this a static global where when non-null, it will use that as the Employee ID and
		//act in Employee/Child mode, unsetting it returns to normal mode
		public static long? EmployeeUserPersonID { get; set; }

		public App()
		{
			try
			{
				Resources["DependencyContainer"] = CreateDependencyContainer();


				GlobalResources.Current.UpdateCurrentDateTime();
				GlobalResources.Current.UpdateLastUserInteraction();

				Device.StartTimer(TimeSpan.FromSeconds(3), () =>
				{
					GlobalResources.Current.UpdateCurrentDateTime();

					var isNormalRunMode =
						App.Current.MainPage != null &&
						App.Current.MainPage.Navigation.NavigationStack.FirstOrDefault() is HomePage;

					if (GlobalResources.Current.HasPageTimedOut && isNormalRunMode)
					{
						//any time they let it idle timeout, it should assume that we leave employee/child mode
						App.EmployeeUserPersonID = null;

						Helpers.Navigation.ResetNavigationAndGoToRoot();
					}

					return true;
				});

				//so this will check for updates on start/restart and then once every day thereafter
				Device.StartTimer(TimeSpan.FromSeconds(10), () =>
				{
					CheckUpdates();
					return false;
				});

				Device.StartTimer(TimeSpan.FromHours(24), () =>
				{
					CheckUpdates();
					return true;
				});

				if (!Helpers.Settings.HasCredentials() || Helpers.Settings.LastSelectedSchoolID == 0) //user never logged in or logged in and did not select school
				{
					MainPage = new NavigationPageHandler(new TimeClock.LoginPage()); // TODO - need to add login page
				}
				else if (Helpers.Settings.LastSelectedSchoolID <= 0) //user has logged in before but no last school saved
				{
					Helpers.Settings.BypassSignatureEmployees = false;
					Helpers.Settings.BypassSignatureParents = false;
					MainPage = new NavigationPageHandler(new TimeClock.StateSelectionPage());
				}
				else //we've got all the info we need to go straight into the app
				{
					MainPage = new NavigationPageHandler(new TimeClock.HomePage());
				}

				SyncEngine.Instance.PullRemoteFinished += Instance_PullRemoteFinished;
				SyncEngine.Instance.PullRemoteStarted += Instance_PullRemoteStarted;
				SyncEngine.Instance.SendRemoteStarted += Instance_SendRemoteStarted;
				SyncEngine.Instance.SendRemoteFinished += Instance_SendRemoteFinished;
			}
			catch (Exception ex)
			{
				TimeClock.Helpers.Logging.Log(ex);
				throw;
			}
		}

		private IServiceCollection CreateDependencyContainer()
		{
			var services = new ServiceCollection();
			// Register services here.

			return services;
		}

		protected override void OnStart()
		{
			// Handle when your app starts
			Helpers.Settings.DeviceID = DeviceInfo.Id;
			var description = String.Format("{0} {1} {2} {4} - App Version: {3}",
				DeviceInfo.Model,
				DeviceInfo.Platform,
				DeviceInfo.VersionString,
				// Assuming you have registered IAutoUpdateHelper in your DI container
				Services.GetService<IAutoUpdateHelper>().GetLocalVersionNumber(),
				DeviceInfo.Idiom);
			Helpers.Settings.DeviceDescription = description;
		}

		protected void CheckUpdates()
		{
			try
			{
				var updateHelper = Microsoft.Maui.DependencyInjection.MauiServices.Current.GetService<IAutoUpdateHelper>();
				if (updateHelper != null)
				{
					if (updateHelper.IsUpdateAvailable())
					{
						updateHelper.DownloadInstallUpdate();
					}
				}
				else
				{
					Logging.Log("Update Helper not found by Dependency Service");
				}
			}
			catch (Exception ex)
			{
				Logging.DebugWrite(ex);
				Logging.Log(ex);
			}
		}
		protected override void OnSleep()
		{
			// Handle when your app sleeps

			//keeping this running if app is literally "exited" seems unneccessary and probably wastes battery life
			SyncEngine.Instance.Stop();
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
			SyncEngine.Instance.Start();

		}
		//note that these are defined as static to allow them to work even if the instance
		//that sets them up goes out of scope
		public static void Instance_SendRemoteFinished(object sender, EventArgs e)
		{
			Debug.WriteLine("Instance_SendRemoteFinished event handler invoked");
		}

		public static void Instance_SendRemoteStarted(object sender, EventArgs e)
		{
			Debug.WriteLine("Instance_SendRemoteStarted event handler invoked");
		}

		public static void Instance_PullRemoteStarted(object sender, EventArgs e)
		{
			Debug.WriteLine("Instance_PullRemoteStarted event handler invoked");
		}

		public static void Instance_PullRemoteFinished(object sender, EventArgs e)
		{
			Debug.WriteLine("Instance_PullRemoteFinished event handler invoked");
		}
	}
}