using System.ComponentModel;
using System.Runtime.CompilerServices;
using Goddard.Clock;

namespace Goddard.Clock.Controls;
public class BaseContentView : ContentView, INotifyPropertyChanged
{
    public double DeviceWidth { get; set; }
    public double DeviceHeight { get; set; }
    public string DeviceType { get; set; }
    public ConstantsStatics.ScreenSize DeviceDisplayInformation { get; set; }
    public DisplayOrientation DeviceOrientation { get; set; }

public new event PropertyChangedEventHandler? PropertyChanged
{
    add { base.PropertyChanged += value; }
    remove { base.PropertyChanged -= value; }
}

    public BaseContentView()
    {
        DeviceType = DeviceInformation.Instance?.DeviceType ?? "small";
        DeviceHeight = DeviceInformation.Instance?.Height ?? 1668;
        DeviceWidth = DeviceInformation.Instance?.Width ?? 2388;
        DeviceDisplayInformation = DeviceInformation.Instance?.DisplayInformation ?? ConstantsStatics.iOSDeviceModels["sm"];
        DeviceOrientation = DeviceInformation.Instance?.GlobalOrientation ?? DisplayOrientation.Landscape;
        if(DeviceInformation.Instance != null){
            DeviceInformation.Instance.PropertyChanged += OnDeviceInformation_PropertyChanged;
        }    }

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
}