using System.ComponentModel;
namespace Goddard.Clock;
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class TimedContentPage : ContentPage, INotifyPropertyChanged
{

    public double DeviceWidth { get; set; }
    public double DeviceHeight { get; set; }
    public string DeviceType { get; set; }

    public ConstantsStatics.ScreenSize DeviceDisplayInformation { get; set; }
    public DisplayOrientation DeviceOrientation { get; set; }


    protected TimedContentPage()
    {
        DeviceType = DeviceInformation.Instance?.DeviceType ?? "small";;
        Shell.SetNavBarIsVisible(this, false);
        DeviceHeight = DeviceInformation.Instance?.Height ?? 1668;
        DeviceWidth = DeviceInformation.Instance?.Width ?? 2388;
        DeviceDisplayInformation = DeviceInformation.Instance?.DisplayInformation ?? ConstantsStatics.iOSDeviceModels["sm"];
        DeviceOrientation = DeviceInformation.Instance?.GlobalOrientation ?? DisplayOrientation.Landscape;
        if(DeviceInformation.Instance != null){
            DeviceInformation.Instance.PropertyChanged += OnDeviceInformation_PropertyChanged;
        }
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
        GlobalResources.Current.GoToMainOnPageTimeout = true;
        GlobalResources.Current.UpdateLastUserInteraction();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}