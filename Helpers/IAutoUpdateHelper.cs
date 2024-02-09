using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeClock.Helpers
{
    public interface IAutoUpdateHelper
    {
        bool IsUpdateAvailable();
        void DownloadInstallUpdate();

        bool IsFirstVersionArgumentLater(string first, string second);

        string GetLocalVersionNumber();
    }
}
