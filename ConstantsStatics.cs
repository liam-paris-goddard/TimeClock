using UIKit;
namespace Goddard.Clock;
public class ConstantsStatics
{
    public static readonly string GoddardDarkHex = "#00447c";
    public static readonly string GoddardMediumHex = "#4e93cc";
    public static readonly string GoddardMediumLightHex = "#79b7e7";
    public static readonly string GoddardLightHex = "#b6daed";
    public static readonly string GoddardAltMediumHex = "#ef3e42";
    public static readonly string GoddardAltLightHex = "#f8b1b3";
    public static readonly string GradientStartingColorHex = "#609fd2";
    public static readonly string GoddardLightestHex = "#ffffff";
    public static readonly string GradientEndingColorHex = "#ffffff";

    public static readonly Color GoddardDarkColor = Color.FromArgb(GoddardDarkHex);
    public static readonly Color GoddardMediumColor = Color.FromArgb(GoddardMediumHex);
    public static readonly Color GoddardMediumLightColor = Color.FromArgb(GoddardMediumLightHex);
    public static readonly Color GoddardLightColor = Color.FromArgb(GoddardLightHex);
    public static readonly Color GoddardAltMediumColor = Color.FromArgb(GoddardAltMediumHex);
    public static readonly Color GoddardAltLightColor = Color.FromArgb(GoddardAltLightHex);
    public static readonly Color GradientStartingColor = Color.FromArgb(GradientStartingColorHex);
    public static readonly Color GoddardLightestColor = Color.FromArgb(GoddardLightestHex);
    public static readonly Color GradientEndingColor = Color.FromArgb(GradientEndingColorHex);

    public static readonly UIColor GoddardDarkUIColor = FromHex(GoddardDarkHex);
    public static readonly UIColor GoddardMediumUIColor = FromHex(GoddardMediumHex);
    public static readonly UIColor GoddardMediumLightUIColor = FromHex(GoddardMediumLightHex);
    public static readonly UIColor GoddardLightUIColor = FromHex(GoddardLightHex);
    public static readonly UIColor GoddardAltMediumUIColor = FromHex(GoddardAltMediumHex);
    public static readonly UIColor GoddardAltLightUIColor = FromHex(GoddardAltLightHex);
    public static readonly UIColor GradientStartingUIColor = FromHex(GradientStartingColorHex);
    public static readonly UIColor GoddardLightestUIColor = FromHex(GoddardLightestHex);
    public static readonly UIColor GradientEndingUIColor = FromHex(GradientEndingColorHex);

    public static readonly float GradientStartingColorLocation;
    public static readonly float GradientEndingColorLocation = 0.87f;



    public class ScreenSize
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double PortraitMagnification { get; set; }
        public double LandscapeMagnification { get; set; }

        public string Key { get; set; }

        public ScreenSize(string key, int width, int height, double basePixels)
        {
            Width = width;
            Height = height;
            PortraitMagnification = (width * height) / basePixels;
            LandscapeMagnification = (height * width) / basePixels;
            Key = key;
        }
    }

    static double basePixels = 2048 * 1536; // iPad Mini 4 resolution

    public static ScreenSize GetScreenSize(string key, int width)
    {
        if (string.IsNullOrEmpty(key))
        {
            // Find the screen size with the closest width
            var closestScreenSize = iOSDeviceModels.Values
                .OrderBy(screenSize => Math.Abs(screenSize.Width - width))
                .First();

            return closestScreenSize;
        }

        return ConstantsStatics.iOSDeviceModels[key];
    }

    public static Dictionary<string, ScreenSize> iOSDeviceModels = new Dictionary<string, ScreenSize>
        {
            // Only unique screen sizes are kept here
            {"mini", new ScreenSize("mini",2048, 1536, basePixels)},
            {"base", new ScreenSize("base",2160, 1620, basePixels)},
            {"md", new ScreenSize("md",2224, 1668, basePixels)},
            {"alt", new ScreenSize("alt",2266, 1488, basePixels)},
            {"air", new ScreenSize("air",2360, 1640, basePixels)},
            {"sm", new ScreenSize("sm",2388, 1668, basePixels)},
            {"Lg", new ScreenSize("Lg",2732, 2048, basePixels)},
        };

    public static Dictionary<string, List<string[]>> modelsByWidth = new Dictionary<string, List<string[]>>()
    {
        { "768", new List<string[]> {
        new string[] {"iPad mini", "1024"},
        new string[] {"iPad 2", "1024"},
        new string[] {"iPad (1st generation)", "1024"},
        }
        },
    { "1488", new List<string[]> { new string[] {"iPad Mini (6th generation)", "2266"},} },
    { "1536", new List<string[]> {
      new string[] {"iPad Mini (5th generation)", "2048"},
      new string[] {"iPad (6th generation)", "2048"},
      new string[] {"iPad (5th generation)", "2048"},
      new string[] {"iPad Pro 9.7-inch (1st generation)", "2048"},
      new string[] {"iPad mini 4", "2048"},
      new string[] {"iPad Air 2", "2048"},
      new string[] {"iPad mini 3", "2048"},
      new string[] {"iPad mini 2", "2048"},
      new string[] {"iPad Air", "2048"},
      new string[] {"iPad (4th generation)", "2048"},
      new string[] {"iPad (3rd generation)", "2048"},
   } },
    { "1620", new List<string[]> {
      new string[] {"iPad (9th generation)", "2160"},
      new string[] {"iPad 8th (generation)", "2160"},
      new string[] {"iPad (7th generation)", "2160"},
   } },
    { "1640", new List<string[]> {
      new string[] {"iPad (10th generation)", "2360"},
      new string[] {"iPad Air (5th generation)", "2360"},
      new string[] {"iPad Air (4th generation)", "2360"},
   } },
    { "1668", new List<string[]> {
      new string[] {"iPad Pro 11-inch (6th generation)", "2388"},
      new string[] {"iPad Pro 11-inch (5th generation)", "2388"},
      new string[] {"iPad Pro 11-inch (4th generation)", "2388"},
      new string[] {"iPad Air (3rd generation)", "2224"},
      new string[] {"iPad Pro 11-inch (3rd generation)", "2388"},
      new string[] {"iPad Pro 10.5-inch (2nd generation)", "2224"},
   } },
    { "2048", new List<string[]> {
      new string[] {"iPad Pro 12.9-inch (6th generation)", "2732"},
      new string[] {"iPad Pro 12.9-inch (5th generation)", "2732"},
      new string[] {"iPad Pro 12.9-inch (4th generation)", "2732"},
      new string[] {"iPad Pro 12.9-inch (3rd generation)", "2732"},
      new string[] {"iPad Pro 12.9-inch (2nd generation)", "2732"},
      new string[] {"iPad Pro 12.9-inch (1st generation)", "2732"},
    }}
  };

    // note that these are also declared in the Goddard.Clock and windows project's app.xaml files
#if PRODBUILD
        public static readonly string PListURL = "https://tabletapi.goddardschool.com/Apps/Install_Goddard.Clock.plist";
        public static readonly string InstallURL = "https://tabletapi.goddardschool.com/Apps/Install.html";
#elif PILOTBUILD
        public static readonly string PListURL = "https://tabletapi.goddardschool.com/Apps/Install_Goddard.Clock.Pilot.plist";
        public static readonly string InstallURL = "https://tabletapi.goddardschool.com/Apps/pilotapp.html";
#elif PILOTQABUILD
        public static readonly string PListURL = "https://tabletapiqa.goddardschool.com/Apps/Install_Goddard.Clock.Pilot.plist";
        public static readonly string InstallURL = "https://tabletapiqa.goddardschool.com/Apps/pilotapp.html";
#else
    public static readonly string PListURL = "https://tabletapiqa.goddardschool.com/Apps/Install_Goddard.Clock.plist";
    public static readonly string InstallURL = "https://tabletapiqa.goddardschool.com/Apps/Install.html";
#endif
    public static readonly string InstallationURL = "itms-services://?action=download-manifest&url=" + PListURL;

    //TODO: increment this each time you deploy a new build, used to notify devices to update

    private static UIColor FromHex(string hexValue)
    {
        var hex = hexValue.Replace("#", "");
        if (hex.Length != 6) throw new ArgumentException("Invalid hex color value.");

        var red = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
        var green = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
        var blue = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

        return UIColor.FromRGB(red, green, blue);
    }

#if PRODBUILD || PILOTBUILD
        public static readonly string TabletWebAPIBaseURL = "https://tabletapi.goddardschool.com/api";
#else
    public static readonly string TabletWebAPIBaseURL = "https://tabletapiqa.goddardschool.com/api";
#endif
}
