using Foundation;
using System.Diagnostics;
using UIKit;

namespace Goddard.Clock.Helpers;
public class AutoUpdateHelper : IAutoUpdateHelper
{
    public async void DownloadInstallUpdate()
    {
        Debug.WriteLine("Downloading and Installing Update");
        var action = await Application.Current.MainPage.DisplayActionSheet("There is an update available, press Update now to open the installation webpage. click anywhere else to dimiss this message",
                "Cancel",
                null,
                "Update now"
        );
        if (action == "Update now")
        {
            // Open the link in a web browser
            await Launcher.OpenAsync(new Uri(ConstantsStatics.InstallURL));
        }
    }

    public string GetLocalVersionNumber()
    {
        //return NSBundle.MainBundle.InfoDictionary?["CFBundleVersion"].ToString();
        return DeployVersion.VersionNumber;
    }

    public async Task<bool> IsUpdateAvailableAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                Debug.WriteLine("Checking for App Update");

                NSMutableDictionary? serverPList = null;
                if (!string.IsNullOrWhiteSpace(ConstantsStatics.PListURL))
                {
                    var url = NSUrl.FromString(ConstantsStatics.PListURL);
                    if (url != null)
                        serverPList = NSMutableDictionary.FromUrl(url);
                }
                if (serverPList != null)
                {
                    if (serverPList["items"] is NSArray items && items.Count > 0)
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

            }
            catch
            {
                return false;
            }

            //everything fell through to here, so assume false
            return false;
        });
    }
    public bool IsFirstVersionArgumentLater(string first, string second)
    {
        Debug.WriteLine(String.Format("Comparing App Versions - First: {0} - Second: {1}", first, second));

        if (String.IsNullOrWhiteSpace(first) || String.IsNullOrWhiteSpace(second) || first.Contains("rc") || second.Contains("rc"))
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
