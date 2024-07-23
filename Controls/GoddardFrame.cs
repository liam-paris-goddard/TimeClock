using Microsoft.Maui.Platform;

namespace Goddard.Clock.Controls;
public class GoddardFrame : Frame
{
    public GoddardFrame()
    {
        BackgroundColor = ConstantsStatics.GoddardMediumLightColor;
        BorderColor = ConstantsStatics.GoddardLightestColor;
        CornerRadius = 14;
    }
}