namespace Goddard.Clock;
using System.ComponentModel;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class UntimedContentPage : ContentPage
{
    public double DeviceWidth { get; set; }
    public double DeviceHeight { get; set; }
    public string DeviceType { get; set; }

    public ConstantsStatics.ScreenSize DeviceDisplayInformation { get; set; }
    public DisplayOrientation DeviceOrientation { get; set; }


    public UntimedContentPage()
    {
        DeviceType = DeviceInformation.Instance?.DeviceType ?? "small";;

        DeviceHeight = DeviceInformation.Instance?.Height ?? 1668;
        DeviceWidth = DeviceInformation.Instance?.Width ?? 2388;
        DeviceDisplayInformation = DeviceInformation.Instance?.DisplayInformation ?? ConstantsStatics.iOSDeviceModels["sm"];
        DeviceOrientation = DeviceInformation.Instance?.GlobalOrientation ?? DisplayOrientation.Landscape;
        if(DeviceInformation.Instance != null){
            DeviceInformation.Instance.PropertyChanged += OnDeviceInformation_PropertyChanged;
        }        Shell.SetNavBarIsVisible(this, false);
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    private void OnDeviceInformation_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DeviceInformation.Width))
            DeviceWidth = DeviceInformation.Instance?.Width ?? 2388;
        if (e.PropertyName == nameof(DeviceInformation.Height))
            DeviceHeight = DeviceInformation.Instance?.Height ?? 1668;
        if (e.PropertyName == nameof(DeviceInformation.DisplayInformation))
            DeviceDisplayInformation = DeviceInformation.Instance?.DisplayInformation ?? ConstantsStatics.iOSDeviceModels["sm"];
        if (e.PropertyName == nameof(DeviceInformation.GlobalOrientation))
            DeviceOrientation = DeviceInformation.Instance?.GlobalOrientation ?? DisplayOrientation.Landscape;
        OnDeviceInformationChanged(e.PropertyName ?? "");
    }

    protected virtual void OnDeviceInformationChanged(string propertyName)
    {
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();
        GlobalResources.Current.GoToMainOnPageTimeout = false;
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}
