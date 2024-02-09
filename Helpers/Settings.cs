using System;
using Microsoft.Maui;
namespace TimeClock.Helpers
{
    public static class Settings
    {
        #region Setting Constants

        private const string UsernameKey = "username_key";
        private static readonly string UsernameDefault = String.Empty;

        private const string PasswordKey = "password_key";
        private static readonly string PasswordDefault = String.Empty;

        private const string LastSelectedSchoolIDKey = "last_selected_school_key";
        private static readonly long LastSelectedSchoolIDDefault = default(long);

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
            get
            {
                return SecureStorage.GetAsync(UsernameKey).Result;
            }
            set
            {
                _ = SecureStorage.SetAsync(UsernameKey, value);
            }
        }

        public static string Password
        {
            get
            {
                return SecureStorage.GetAsync(PasswordKey).Result;
            }
            set
            {
                _ = SecureStorage.SetAsync(PasswordKey, value);
            }
        }

        public static long LastSelectedSchoolID
        {
            get
            {
                return Preferences.Get(LastSelectedSchoolIDKey, LastSelectedSchoolIDDefault);
            }
            set
            {
                Preferences.Set(LastSelectedSchoolIDKey, value);
            }
        }

        public static string LastSelectedSchoolName
        {
            get
            {
                return Preferences.Get(LastSelectedSchoolNameKey, LastSelectedSchoolNameDefault);
            }
            set
            {
                Preferences.Set(LastSelectedSchoolNameKey, value);
            }
        }

        public static string DeviceID
        {
            get
            {
                return Preferences.Get(DeviceIDKey, DeviceIDDefault);
            }
            set
            {
                Preferences.Set(DeviceIDKey, value);
            }
        }

        public static string DeviceDescription
        {
            get
            {
                return Preferences.Get(DeviceDescriptionKey, DeviceDescriptionDefault);
            }
            set
            {
                Preferences.Set(DeviceDescriptionKey, value);
            }
        }

        public static bool BypassSignatureEmployees
        {
            get
            {
                return Preferences.Get(BypassSignatureEmployeesKey, BypassSignatureEmployeesDefault);
            }
            set
            {
                Preferences.Set(BypassSignatureEmployeesKey, value);
            }
        }

        public static bool BypassSignatureParents
        {
            get
            {
                return Preferences.Get(BypassSignatureParentsKey, BypassSignatureParentsDefault);
            }
            set
            {
                Preferences.Set(BypassSignatureParentsKey, value);
            }
        }

        public static bool IsMultiSchoolUser
        {
            get
            {
                return Preferences.Get(IsMultiSchoolUserKey, IsMultiSchoolUserDefault);
            }
            set
            {
                Preferences.Set(IsMultiSchoolUserKey, value);
            }
        }
    }
}

/**
 TODO - consider

this might really need to be changed or change wherever it is referenced

Improvements and modern C# features:

Async Properties: The Username and Password properties use SecureStorage.GetAsync().Result which can cause deadlocks. Consider changing your design to use methods instead of properties for these settings, so you can use await.

Encapsulation: Consider encapsulating the keys and default values in a separate class or struct. This can make the code more organized and easier to maintain.

Nullability Annotations: Consider using nullability annotations to make your code more robust and less prone to null reference exceptions.

*/