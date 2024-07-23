using Goddard.Clock.Controls;
using Microsoft.Maui.Handlers;
using UIKit;
using CoreAnimation;
using Foundation;
using Microsoft.Maui.Platform;


namespace Goddard.Clock.Handlers;

public class GradientImageButton : UIImageView
{
    readonly CAGradientLayer gradientLayer;
    public bool useAltColor;

    public bool persistAltColor;

    public GradientImageButton(bool persistAltColorInput = false)
    {
        persistAltColor = persistAltColorInput;
        gradientLayer = new CAGradientLayer();
        Layer.InsertSublayer(gradientLayer, 0);
        UpdateBackgroundColor(this, persistAltColor);
    }

    public override void LayoutSubviews()
    {
        base.LayoutSubviews();

        gradientLayer.Frame = Bounds;
        if(persistAltColor) {
            UpdateBackgroundColor(this, persistAltColor);
        } else {
            UpdateBackgroundColor(this, useAltColor);
        }
    }
    public void UpdateBackgroundColor(UIImageView button, bool inputAltColor = false )
    {
        // Remove old gradient layers
        var oldGradientLayers = button.Layer.Sublayers?.Where(layer => layer is CAGradientLayer).ToArray();
        if (oldGradientLayers != null)
        {
            foreach (var oldLayer in oldGradientLayers)
            {
                oldLayer.RemoveFromSuperLayer();
            }
        }

        // Create and add new gradient layer for background
        var gradientLayer = new CAGradientLayer();
        gradientLayer.Frame = button.Bounds;
        var startColor = inputAltColor ? ConstantsStatics.GoddardAltLightColor.ToCGColor() : ConstantsStatics.GoddardLightColor.ToCGColor();
        var endColor = inputAltColor ? ConstantsStatics.GoddardAltMediumColor.ToCGColor() : ConstantsStatics.GoddardMediumColor.ToCGColor();
        gradientLayer.Colors = new[] { startColor, endColor };
        gradientLayer.Locations = new NSNumber[] { 0.0, 0.35 };
        button.Layer.InsertSublayer(gradientLayer, 0);
    }

}
public partial class GoddardImageButtonHandler : ButtonHandler
{

    static IPropertyMapper<GoddardImageButton, GoddardImageButtonHandler> PropertyMapper = new PropertyMapper<GoddardImageButton, GoddardImageButtonHandler>(Mapper)
    {
        ["UseAltColor"] = (handler, view) => handler.UpdateBackgroundColor(handler.PlatformView, view.UseAltColor),
        ["PersistAltColor"] = (handler, view) => handler.UpdateBackgroundColorPersist(handler.PlatformView, view.PersistAltColor),
        ["CornerRadius"] = (handler, view) => handler.UpdateCornerRadius(handler.PlatformView, view.CornerRadius),
        ["BorderColor"] = (handler, view) => handler.UpdateBorderColor(handler.PlatformView, view.BorderColor),
        ["BorderWidth"] = (handler, view) => handler.UpdateBorderWidth(handler.PlatformView, view.BorderWidth),
        ["BackgroundColor"] = (handler, view) => handler.UpdateBackgroundColor(handler.PlatformView),
    };
    public GoddardImageButtonHandler() : base(PropertyMapper)
    {

    }
    protected override UIButton CreatePlatformView()
    {

        var button = new GradientButton();

        button.TouchDown += OnTouchDown;
        button.TouchUpInside += OnTouchUpInside;
        button.TouchUpOutside += OnTouchUpInside;
        button.Layer.MasksToBounds = true;
        button.Layer.CornerRadius = 8;
        button.Layer.BorderWidth = 0;
        button.SetTitleColor(ConstantsStatics.GoddardLightestUIColor, UIControlState.Normal);
        button.SetTitleColor(ConstantsStatics.GoddardLightestUIColor, UIControlState.Highlighted);
        button.SetTitleColor(ConstantsStatics.GoddardLightestUIColor, UIControlState.Selected);
        button.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
        button.TitleLabel.TextAlignment = UITextAlignment.Center;
        button.TitleLabel.Font = UIFont.FromName("HelveticaNeue-Bold", (nfloat)10);
        button.AdjustsImageWhenHighlighted = false;
        return button;
    }

    protected override void ConnectHandler(UIButton nativeView)
    {
        UpdateBackgroundColor(nativeView);
        base.ConnectHandler(nativeView);

    }

    protected override void DisconnectHandler(UIButton nativeView)
    {
        base.DisconnectHandler(nativeView);
        nativeView.TouchDown -= OnTouchDown;
        nativeView.TouchUpInside -= OnTouchUpInside;
        nativeView.TouchUpOutside -= OnTouchUpInside;
    }

    private void OnTouchDown(object? sender, EventArgs e)
    {
        var button = sender as GradientButton;
        if (button != null && !button.persistAltColor)
        {
            button.useAltColor = true;
            button.UpdateBackgroundColor(button, button.useAltColor);
        }
    }

    private void OnTouchUpInside(object? sender, EventArgs e)
    {
        var button = sender as GradientButton;
        if (button != null && !button.persistAltColor)
        {
            button.useAltColor = false;
            button.UpdateBackgroundColor(button, button.useAltColor);
        }
    }

    private void UpdateCornerRadius(UIButton button, double cornerRadius)
    {
        button.Layer.CornerRadius = (nfloat)cornerRadius;
    }


    private void UpdateText(UIButton button, string text)
    {
        button.SetTitle(text, UIControlState.Normal);
    }

    private void UpdateBorderWidth(UIButton button, double borderWidth)
    {
        button.Layer.BorderWidth = (nfloat)borderWidth;
    }

    private void UpdateBorderColor(UIButton button, Color borderColor)
    {
        if (borderColor != null)
            button.Layer.BorderColor = borderColor.ToCGColor();
        else
            button.Layer.BorderColor = UIColor.White.CGColor;

    }
    private void UpdateTextColor(UIButton button, Color textColor)
    {
        button.SetTitleColor(ConstantsStatics.GoddardLightestUIColor, UIControlState.Normal);
    }
    private void UpdateFont(UIButton button, double fontSize)
    {
        button.TitleLabel.Font = UIFont.FromName("HelveticaNeue-Bold", (nfloat)fontSize);
    }

    private void UpdateBackgroundColorPersist(UIButton sentButton, bool persistAltColorInput)
    {
        var button = sentButton as GradientButton;
        button.persistAltColor = persistAltColorInput;
        button.UpdateBackgroundColor(button, persistAltColorInput);
    }

    private void UpdateBackgroundColor(UIButton button, bool inputAltColor = false)
    {
        // Remove old gradient layers
        var oldGradientLayers = button.Layer.Sublayers?.Where(layer => layer is CAGradientLayer).ToArray();
        if (oldGradientLayers != null)
        {
            foreach (var oldLayer in oldGradientLayers)
            {
                oldLayer.RemoveFromSuperLayer();
            }
        }

        // Create and add new gradient layer for background
        var gradientLayer = new CAGradientLayer();
        gradientLayer.Frame = button.Bounds;
        var startColor = inputAltColor ? ConstantsStatics.GoddardAltLightColor.ToCGColor() : ConstantsStatics.GoddardLightColor.ToCGColor();
        var endColor = inputAltColor ? ConstantsStatics.GoddardAltMediumColor.ToCGColor() : ConstantsStatics.GoddardMediumColor.ToCGColor();
        gradientLayer.Colors = new[] { startColor, endColor };
        gradientLayer.Locations = new NSNumber[] { 0.0, 0.35 };
        button.Layer.InsertSublayer(gradientLayer, 0);
    }
}
