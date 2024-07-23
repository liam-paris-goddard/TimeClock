using Goddard.Clock.Models;

namespace Goddard.Clock.Factories;
public interface IPreCheckInPageFactory
{
    PreCheckInPage Create(UserType personType, long userPersonId, string userFirstName, string userLastName, long? employeeSelectedFamilyId = null);
}

public class PreCheckInPageFactory : IPreCheckInPageFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PreCheckInPageFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public PreCheckInPage Create(UserType personType, long userPersonId, string userFirstName, string userLastName, long? employeeSelectedFamilyId = null)
    {
        return new PreCheckInPage(personType, userPersonId, userFirstName, userLastName, _serviceProvider, employeeSelectedFamilyId);
    }
}