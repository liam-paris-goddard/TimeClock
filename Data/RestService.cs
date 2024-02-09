using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.Http;
using RestSharp;
using RestSharp.Authenticators;
using TimeClock.Models;
using CommunityToolkit.Maui;
namespace TimeClock.Data
{
    public class RestService
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string BaseURL { get; private set; }

        public RestService()
            : this(Helpers.Settings.Username, Helpers.Settings.Password, ConstantsStatics.TabletWebAPIBaseURL)
        {
        }

        public RestService(string username, string password, string baseURL)
        {
            Username = username;
            Password = password;
            BaseURL = baseURL;
        }

        private async Task<bool> ExecPostMethod(string url, object data)
        {
            try
            {
                var options = new RestClientOptions(BaseURL)
                {
                    Authenticator = new HttpBasicAuthenticator(Username, Password)
                };
                var client = new RestClient(options);
                var request = new RestRequest(url, Method.Post);

                request.AddOrUpdateHeader("x-fms-schoolID", Helpers.Settings.LastSelectedSchoolID);
                request.AddOrUpdateHeader("x-fms-deviceID", Helpers.Settings.DeviceID);
                request.AddOrUpdateHeader("x-fms-deviceDescription", Helpers.Settings.DeviceDescription);

                request.AddJsonBody(data);

                var result = await client.ExecuteAsync(request);
                if (result.IsSuccessful && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<T?> ExecGetMethod<T>(string format, params object[] value)
        {
            try
            {
                var options = new RestClientOptions(BaseURL)
                {
                    Authenticator = new HttpBasicAuthenticator(Username, Password)
                };
                var client = new RestClient(options);
                var methodURL = String.Format(format, value);
                var request = new RestRequest(methodURL, Method.Get);

                request.AddOrUpdateHeader("x-fms-schoolID", Helpers.Settings.LastSelectedSchoolID);
                request.AddOrUpdateHeader("x-fms-deviceID", Helpers.Settings.DeviceID);
                request.AddOrUpdateHeader("x-fms-deviceDescription", Helpers.Settings.DeviceDescription);

                var result = await client.ExecuteAsync<T>(request);
                if (result.IsSuccessful && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return result.Data;
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }
        public async Task<SchoolConfiguration> GetSchoolConfiguration(long schoolID)
        {
            return await ExecGetMethod<SchoolConfiguration>("user/schoolconfig/{0}", schoolID);
        }

        public async Task<AllowedSchool[]> GetAllowedSchools(string user)
        {
            var cipher = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(user));
            return await ExecGetMethod<AllowedSchool[]>("user/allowedschools/{0}", cipher);
        }

        public async Task<PullSyncData> GetSyncData(long schoolID)
        {
            return await ExecGetMethod<PullSyncData>("user/pullsyncdata/{0}", schoolID);
        }

        public async Task<bool> SendSyncData(SendSyncData myData)
        {
            return await ExecPostMethod("user/sendsyncdata/", myData);
        }

        public async Task<bool> RegisterPullCompleted()
        {
            return await ExecGetMethod<bool>("user/pullcompleted/");
        }

        public async Task<bool> SendLogs(LocalLog[] logs)
        {
            return await ExecPostMethod("user/sendlogs/", logs);
        }

        public async Task<Parent[]> AuthenticateParent(string namestart, string pin)
        {
            return await ExecGetMethod<Parent[]>($"user/authenticateparent?namestart={namestart}&pin={pin}");
        }

        public async Task<Employee> AuthenticateEmployee(long employeeID, string pin)
        {
            return await ExecGetMethod<Employee>($"user/authenticateemployee?employeeid={employeeID}&pin={pin}");
        }
    }
}
