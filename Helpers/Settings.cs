using System;
using Microsoft.Maui.Essentials;

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
                SecureStorage.SetAsync(UsernameKey, value);
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
                SecureStorage.SetAsync(PasswordKey, value);
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
TODO 

hardcoded-credentials Embedding credentials in source code risks unauthorized access
clear-text-storage-of-sensitive-data
Please note that SecureStorage.GetAsync and SecureStorage.SetAsync are asynchronous methods, so you should ideally use await when calling them. However, since properties can't be async, I've used .Result to get the result of the task. This can potentially cause deadlocks in certain scenarios, so you might want to consider changing your design to avoid this.
*/