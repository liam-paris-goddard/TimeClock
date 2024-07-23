using System.Diagnostics;
using System.ComponentModel;
using Goddard.Clock.Data;
using Goddard.Clock.Helpers;
using Goddard.Clock.Factories;
using Goddard.Clock.Models;
using Goddard.Clock;

namespace Goddard.Clock;
public class DeviceInformation
{
    private static DeviceInformation? _instance;
    public static DeviceInformation Instance
    {
        get
        {
            _instance ??= new DeviceInformation();
            return _instance;
        }
    }

    private DisplayOrientation _globalOrientation;
    public DisplayOrientation GlobalOrientation
    {
        get { return _globalOrientation; }
        set
        {
            if (_globalOrientation != value)
            {
                _globalOrientation = value;
                OnPropertyChanged(nameof(GlobalOrientation));
            }
        }
    }

    private ConstantsStatics.ScreenSize? _displayInformation;
    public ConstantsStatics.ScreenSize? DisplayInformation
    {
        get { return _displayInformation; }
        set
        {
            if (_displayInformation != value)
            {
                _displayInformation = value;
                OnPropertyChanged(nameof(DisplayInformation));
            }
        }
    }

    private double _width;
    public double Width
    {
        get { return _width; }
        set
        {
            if (_width != value)
            {
                _width = value;
                OnPropertyChanged(nameof(Width));
            }
        }
    }

    private double _height;
    public double Height
    {
        get { return _height; }
        set
        {
            if (_height != value)
            {
                _height = value;
                OnPropertyChanged(nameof(Height));
            }
        }
    }

    private string? _deviceType;
    public string? DeviceType
    {
        get { return _deviceType; }
        set
        {
            if (_deviceType != value)
            {
                _deviceType = value;
                OnPropertyChanged(nameof(DeviceType));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}