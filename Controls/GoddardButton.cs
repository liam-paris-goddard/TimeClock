namespace Goddard.Clock.Controls;
public class GoddardButton : Button
{
    public static readonly BindableProperty UseAltColorProperty = BindableProperty.Create(nameof(UseAltColor), typeof(bool), typeof(GoddardButton), false);
    public static readonly BindableProperty PersistAltColorProperty = BindableProperty.Create(nameof(PersistAltColor), typeof(bool), typeof(GoddardButton), false);


    public bool UseAltColor
    {
        get => (bool)GetValue(UseAltColorProperty);
        set => SetValue(UseAltColorProperty, value);
    }

    public bool PersistAltColor
    {
        get => (bool)GetValue(PersistAltColorProperty);
        set => SetValue(PersistAltColorProperty, value);
    }

    public GoddardButton()
    {
        Clicked += GoddardButton_Clicked;
        this.FontAttributes = FontAttributes.Bold;
        this.FontFamily = "HelveticaNeue-Bold"; // Set the font family
        this.BorderWidth = 4;
        this.CornerRadius = 8;
        this.BorderColor = Colors.White;

    }

    private void GoddardButton_Clicked(object? sender, EventArgs e)
    {
        GlobalResources.Current.UpdateLastUserInteraction();
    }
}
