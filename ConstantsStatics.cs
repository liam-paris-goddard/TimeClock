using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace TimeClock
{
    public class ConstantsStatics
    {
        public static readonly string GradientStartingColor = "609fd2";
        public static readonly double GradientStartingColorLocation = 0.0;
        public static readonly double GradientEndingColorLocation = 0.87;
        public static readonly string GradientEndingColor = "ffffff";

        // note that these are also declared in the TimeClock and windows project's app.xaml files
        public static readonly Color GoddardDark = Color.FromArgb("#00447c");
        public static readonly Color GoddardMedium = Color.FromArgb("#4e93cc");
        public static readonly Color GoddardMediumLight = Color.FromArgb("#79b7e7");
        public static readonly Color GoddardLight = Color.FromArgb("#b6daed");
        public static readonly Color GoddardAltMedium = Color.FromArgb("#ef3e42");
        public static readonly Color GoddardAltLight = Color.FromArgb("#f8b1b3");
        public static readonly Color GoddardLightest = Color.FromArgb("#ffffff");

        public static readonly string PListURL = "https://tabletapi.goddardschool.com/Apps/Install_Timeclock.plist";
        public static readonly string InstallationURL = "itms-services://?action=download-manifest&url=" + PListURL;

        //TODO: increment this each time you deploy a new build, used to notify devices to update
        public static readonly string DeployVersion = "2.0.5";

#if DEBUG
        public static readonly string TabletWebAPIBaseURL = "https://tabletapitest.goddardschool.com/api";
        //public static readonly string TabletWebAPIBaseURL = "http://localhost:50474/api";
#else 
        public static readonly string TabletWebAPIBaseURL = "https://tabletapi.goddardschool.com/api";
#endif
    }
}