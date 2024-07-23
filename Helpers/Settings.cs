namespace Goddard.Clock.Helpers;
public static class Settings
{
    #region Setting Constants

    private const string UsernameKey = "username_key";
    private static readonly string UsernameDefault = String.Empty;

    private const string PasswordKey = "password_key";
    private static readonly string PasswordDefault = String.Empty;

    private const string LastSelectedSchoolIDKey = "last_selected_school_key";
    private static readonly long LastSelectedSchoolIDDefault = default;

    private const string LastSelectedSchoolNameKey = "last_selected_school_name_key";
    private static readonly string LastSelectedSchoolNameDefault = String.Empty;

    private const string DeviceIDKey = "device_id_key";
    private static readonly string DeviceIDDefault = String.Empty;

    private const string DeviceDescriptionKey = "device_description_key";
    private static readonly string DeviceDescriptionDefault = String.Empty;

    private const string BypassSignatureEmployeesKey = "bypasssignatureemployees_key";
    private static readonly bool BypassSignatureEmployeesDefault = false;

    private const string BypassSignatureParentsKey = "bypasssignatureparents_key";
    private static readonly bool BypassSignatureParentsDefault = false;
    private const string IsMultiSchoolUserKey = "is_multi_school_user_key";
    private static readonly bool IsMultiSchoolUserDefault = false;

    #endregion

    public static bool HasCredentials()
    {
        return !String.IsNullOrWhiteSpace(Username) && !String.IsNullOrWhiteSpace(Password);
    }

    public static string Username
    {
        get => SecureStorage.GetAsync(UsernameKey).Result!;
        set => _ = SecureStorage.SetAsync(UsernameKey, value);
    }

    public static string Password
    {
        get => SecureStorage.GetAsync(PasswordKey).Result!;
        set => _ = SecureStorage.SetAsync(PasswordKey, value);
    }

    public static long LastSelectedSchoolID
    {
        get => Preferences.Get(LastSelectedSchoolIDKey, LastSelectedSchoolIDDefault);
        set => Preferences.Set(LastSelectedSchoolIDKey, value);
    }

    public static string LastSelectedSchoolName
    {
        get => Preferences.Get(LastSelectedSchoolNameKey, LastSelectedSchoolNameDefault);
        set => Preferences.Set(LastSelectedSchoolNameKey, value);
    }

    public static string DeviceID
    {
        get => Preferences.Get(DeviceIDKey, DeviceIDDefault);
        set => Preferences.Set(DeviceIDKey, value);
    }

    public static string DeviceDescription
    {
        get => Preferences.Get(DeviceDescriptionKey, DeviceDescriptionDefault);
        set => Preferences.Set(DeviceDescriptionKey, value);
    }

    public static bool BypassSignatureEmployees
    {
        get => Preferences.Get(BypassSignatureEmployeesKey, BypassSignatureEmployeesDefault);
        set => Preferences.Set(BypassSignatureEmployeesKey, value);
    }

    public static bool BypassSignatureParents
    {
        get => Preferences.Get(BypassSignatureParentsKey, BypassSignatureParentsDefault);
        set => Preferences.Set(BypassSignatureParentsKey, value);
    }

    public static bool IsMultiSchoolUser
    {
        get => Preferences.Get(IsMultiSchoolUserKey, IsMultiSchoolUserDefault);
        set => Preferences.Set(IsMultiSchoolUserKey, value);
    }
}