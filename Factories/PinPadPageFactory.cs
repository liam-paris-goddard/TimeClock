using Goddard.Clock.Models;

namespace Goddard.Clock.Factories;
public interface IPinPadPageFactory
{
    PinPadPage Create(UserType userType, string personName, long? personId = null, bool setPINMode = false, string originalPIN = "", bool showPinResetMessageOnLoad = false);
}

public class PinPadPageFactory : IPinPadPageFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PinPadPageFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public PinPadPage Create(UserType userType, string personName, long? personId = null, bool setPINMode = false, string originalPIN = "", bool showPinResetMessageOnLoad = false)
    {
        return new PinPadPage(_serviceProvider, userType, personName, personId, setPINMode, originalPIN, showPinResetMessageOnLoad);
    }
}