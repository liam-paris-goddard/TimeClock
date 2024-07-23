namespace Goddard.Clock.Factories;
public interface IHomePageFactory
{
    HomePage Create(bool showWelcomeImage = true);
}

public class HomePageFactory : IHomePageFactory
{
    private readonly IServiceProvider _serviceProvider;

    public HomePageFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public HomePage Create(bool showWelcomeImage = true)
    {
        return new HomePage(_serviceProvider, showWelcomeImage);
    }
}
