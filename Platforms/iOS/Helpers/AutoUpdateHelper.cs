using Foundation;
using TimeClock.Helpers;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using UIKit;
using System.Security.Cryptography;

namespace TimeClock.iOS.Helpers
{
    public class AutoUpdateHelper : IAutoUpdateHelper
    {
        public void DownloadInstallUpdate()
        {
            Debug.WriteLine("Downloading and Installing Update");
            UIApplication.SharedApplication.OpenUrl(NSUrl.FromString(ConstantsStatics.InstallationURL));
        }

        public string GetLocalVersionNumber()
        {
            //return NSBundle.MainBundle.InfoDictionary?["CFBundleVersion"].ToString();
            return ConstantsStatics.DeployVersion;
        }

        public bool IsUpdateAvailable()
        {
            try
            {
                Debug.WriteLine("Checking for App Update");

                var serverPList = NSDictionary.FromUrl(NSUrl.FromString(ConstantsStatics.PListURL));
                var items = serverPList["items"] as NSArray;
                if (items != null && items.Count > 0)
                {
                    var itemDictionary = items.GetItem<NSDictionary>(items.Count - 1);
                    if (itemDictionary != null)
                    {
                        var metadata = itemDictionary["metadata"] as NSDictionary;
                        if (metadata != null)
                        {
                            var serverVersion = metadata["bundle-version"]?.ToString();
                            if (!String.IsNullOrWhiteSpace(serverVersion))
                            {
                                var localVersion = GetLocalVersionNumber();
                                return IsFirstVersionArgumentLater(serverVersion, localVersion);
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            //everything fell through to here, so assume false
            return false;
        }



        public bool IsFirstVersionArgumentLater(string first, string second)
        {
            Debug.WriteLine(String.Format("Comparing App Versions - First: {0} - Second: {1}", first, second));

            if (String.IsNullOrWhiteSpace(first) || String.IsNullOrWhiteSpace(second))
            {
                return false;
            }
            else if (first.ToLowerInvariant().Trim() == second.ToLowerInvariant().Trim())
            {
                return false;
            }
            else
            {
                return Version.Parse(first) > Version.Parse(second);
            }

        }
    }
}


/** 

todo - consider

need to add to startup.Cs

using TimeClock.iOS.Helpers;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace TimeClock
{
    public class Startup : IStartup
    {
        public void Configure(IAppHostBuilder appBuilder)
        {
            appBuilder
                .UseMauiApp<App>()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IAutoUpdateHelper, AutoUpdateHelper>();
                });
        }
    }
}*/