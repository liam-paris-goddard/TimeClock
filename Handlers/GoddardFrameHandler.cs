using Goddard.Clock.Controls;
using Microsoft.Maui.Handlers;
using UIKit;
using CoreAnimation;
using Foundation;
using Microsoft.Maui.Platform;


namespace Goddard.Clock.Handlers;
public partial class GoddardFrameHandler : ContentViewHandler
{

    static IPropertyMapper<GoddardFrame, GoddardFrameHandler> PropertyMapper = new PropertyMapper<GoddardFrame, GoddardFrameHandler>(Mapper)
    {
        [nameof(GoddardFrame.BackgroundColor)] = (handler, view) => handler.MapBackgroundColor(handler.PlatformView),
        [nameof(GoddardFrame.BorderColor)] = (handler, view) => handler.MapBorderColor(handler.PlatformView),
        [nameof(GoddardFrame.CornerRadius)] = (handler, view) => handler.MapCornerRadius(handler.PlatformView)

    };
    public GoddardFrameHandler() : base(PropertyMapper)
    {

    }
    protected override Microsoft.Maui.Platform.ContentView CreatePlatformView()
    {

        var frame = new Microsoft.Maui.Platform.ContentView();
        frame.Layer.BorderColor = ConstantsStatics.GoddardLightestColor.ToCGColor();
        frame.Layer.BorderWidth = (float)6;
        frame.Layer.CornerRadius = (float)14;
        frame.Layer.BackgroundColor = ConstantsStatics.GoddardMediumLightColor.ToCGColor();
        return frame;
    }

    protected override void ConnectHandler(Microsoft.Maui.Platform.ContentView nativeView)
    {
        base.ConnectHandler(nativeView);

    }

    protected override void DisconnectHandler(Microsoft.Maui.Platform.ContentView nativeView)
    {
        base.DisconnectHandler(nativeView);
    }

    public void MapBackgroundColor(Microsoft.Maui.Platform.ContentView nativeView)
    {
        nativeView.BackgroundColor = ConstantsStatics.GoddardMediumLightUIColor;
    }

    public void MapBorderColor(Microsoft.Maui.Platform.ContentView nativeView)
    {

        // Apply border color. You might need a more specific UIKit view type for detailed properties.
        nativeView.Layer.BorderColor = ConstantsStatics.GoddardLightestColor.ToCGColor();
    }

    public void MapCornerRadius(Microsoft.Maui.Platform.ContentView nativeView)
    {

        nativeView.Layer.CornerRadius = (float)14;
    }

    public void MapBorderWidth(Microsoft.Maui.Platform.ContentView nativeView)
    {
        nativeView.Layer.BorderWidth = (float)6;
    }
}