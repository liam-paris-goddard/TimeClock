namespace Goddard.Clock.Helpers;
public interface IAutoUpdateHelper
{
    Task<bool> IsUpdateAvailableAsync();
    void DownloadInstallUpdate();

    bool IsFirstVersionArgumentLater(string first, string second);

    string GetLocalVersionNumber();
}
