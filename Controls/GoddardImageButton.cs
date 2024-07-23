namespace Goddard.Clock.Controls;
public class GoddardImageButton : ImageButton
{
    public static readonly BindableProperty UseAltColorProperty = BindableProperty.Create(nameof(UseAltColor), typeof(bool), typeof(GoddardImageButton), false);
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

    public GoddardImageButton()
    {
        Clicked += GoddardImageButton_Clicked;
        this.Aspect = Aspect.Fill;
        this.BorderWidth = 4;
        this.CornerRadius = 8;
        this.BorderColor = Colors.White;

    }

    private void GoddardImageButton_Clicked(object? sender, EventArgs e)
    {
        GlobalResources.Current.UpdateLastUserInteraction();
    }
}
