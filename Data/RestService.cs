using System.Text;
using RestSharp;
using Goddard.Clock.Models;

namespace Goddard.Clock.Data;
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

    //note on exception handling in two methods below - the reality is there is nothing to be done about
    //exceptions at this level because it's almost certainly some unrecoverable at the moment but transitory
    //issue like network access/temporary interuption or wrong username/password and so on and logging the 
    //errors can produce a load of useless noise in the logs
    private async Task<bool> ExecPostMethod(string url, object data)
    {
        try
        {
            using (var client = new RestClient(BaseURL))
            {
                var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}"));
                _ = client.AddDefaultHeader("Authorization", $"Basic {encodedCredentials}");
                var request = new RestRequest(url, Method.Post);

                _ = request.AddOrUpdateHeader("x-fms-schoolID", Helpers.Settings.LastSelectedSchoolID);
                _ = request.AddOrUpdateHeader("x-fms-deviceID", Helpers.Settings.DeviceID);
                _ = request.AddOrUpdateHeader("x-fms-deviceDescription", Helpers.Settings.DeviceDescription);

                _ = request.AddJsonBody(data);

                var result = await client.ExecuteAsync(request);
                if (result.IsSuccessful && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    //set note above
                    //Debug.WriteLine(@"RESTful GET failed: {0} - {1}", result.StatusCode, result.StatusDescription);
                    return false;
                }
            }
        }
        catch
        {
            //set note above
            //Goddard.Clock.Helpers.Logging.Log(ex, "RESTful POST exception");
            return false;
        }
    }

    private async Task<T?> ExecGetMethod<T>(string format, params object[] value)
    {
        try
        {
            using (var client = new RestClient(BaseURL))
            {
                var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}"));
                _ = client.AddDefaultHeader("Authorization", $"Basic {encodedCredentials}");
                var methodURL = String.Format(format, value);
                var request = new RestRequest(methodURL, Method.Get);

                _ = request.AddOrUpdateHeader("x-fms-schoolID", Helpers.Settings.LastSelectedSchoolID);
                _ = request.AddOrUpdateHeader("x-fms-deviceID", Helpers.Settings.DeviceID);
                _ = request.AddOrUpdateHeader("x-fms-deviceDescription", Helpers.Settings.DeviceDescription);

                var result = await client.ExecuteAsync<T>(request);
                if (result.IsSuccessful && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return result.Data;
                }
                else
                {
                    //set note above
                    //Debug.WriteLine(@"RESTful GET failed: {0} - {1}", result.StatusCode, result.StatusDescription);
                    return default;
                }
            }
        }
        catch
        {
            //set note above
            //Goddard.Clock.Helpers.Logging.Log(ex, "RESTful GET exception");
            return default;
        }
    }

    public async Task<SchoolConfiguration> GetSchoolConfiguration(long schoolID)
    {
        var result = await ExecGetMethod<SchoolConfiguration>("user/schoolconfig/{0}", schoolID);
        return result;
    }

    public async Task<AllowedSchool[]> GetAllowedSchools(string user)
    {
        var cipher = Convert.ToBase64String(Encoding.UTF8.GetBytes(user));
        var result = await ExecGetMethod<AllowedSchool[]>("user/allowedschools/{0}", cipher);
        return result;
    }

    public async Task<PullSyncData> GetSyncData(long schoolID)
    {
        var result = await ExecGetMethod<PullSyncData>("user/pullsyncdata/{0}", schoolID);
        return result;
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
        var result = await ExecGetMethod<Parent[]>($"user/authenticateparent?namestart={namestart}&pin={pin}");
        return result;
    }

    public async Task<Employee> AuthenticateEmployee(long employeeID, string pin)
    {
        var result = await ExecGetMethod<Employee>($"user/authenticateemployee?employeeid={employeeID}&pin={pin}");
        return result;
    }
}
