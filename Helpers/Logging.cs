using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeClock.Helpers
{
    public class Logging
    {
        public async static Task Log(Exception ex, string message = "", string? parentExceptionID = null)
        {
            try
            {
                if (ex == null)
                    return;

                _ = DebugWrite(ex, message, parentExceptionID);

                var dict = "";
                if (ex.Data?.Count > 0)
                {
                    foreach (DictionaryEntry de in ex.Data)
                        dict += $"    Key: {'"' + de.Key.ToString() + '"',-20}      Value: {de.Value}\n";
                }

                //first log the current level
                var log = new Models.LocalLog()
                {
                    Occurred = DateTime.Now,
                    GloballyUniqueID = Guid.NewGuid().ToString(),
                    HelpLink = ex.HelpLink,
                    HResult = ex.HResult,
                    InnerExceptionID = parentExceptionID,
                    ExceptionMessage = ex.Message,
                    Source = ex.Source,
                    StackTrace = ex.StackTrace,
                    OptionalMessage = message,
                    ExceptionData = dict
                };

                await App.Database.LocalLogInsert(log);

                //if inner except, recursively log it too
                if (ex?.InnerException != null)
                    _ = Log(ex.InnerException, message, log.GloballyUniqueID);

                if (ex is AggregateException agg)
                {
                    foreach (var inner in agg.InnerExceptions)
                        _ = Log(inner, message, log.GloballyUniqueID);


                    agg.Handle(ae =>
                    {
                        _ = Log(ae, message, log.GloballyUniqueID);
                        return true;
                    });
                }
            }
            catch (Exception logException)
            {
                _ = DebugWrite(logException, "Exception during TimeClock.Helpers.Logging.Log(exception, string, string)", parentExceptionID);
            }

        }

        public async static Task Log(string message)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(message))
                    return;

                _ = DebugWrite(message);

                var log = new Models.LocalLog()
                {
                    Occurred = DateTime.Now,
                    GloballyUniqueID = Guid.NewGuid().ToString(),
                    OptionalMessage = message
                };

                await App.Database.LocalLogInsert(log);
            }
            catch (Exception logException)
            {
                _ = DebugWrite(logException, "Exception during TimeClock.Helpers.Logging.Log(string)");
            }
        }

        public static async Task DebugWrite(Exception ex, string message = "", string? parentExceptionID = null)
        {
            try
            {
                if (ex == null)
                    return;

                Debug.WriteLine($"EXCEPTION: {ex.Message}\n{message}\n{ex.Source}\n{ex.StackTrace}");
            }
            catch
            {
                //at this point we're stuck, so just no-op
            }
        }

        public static Task DebugWrite(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                    return Task.CompletedTask;
                Debug.WriteLine($"MESSAGE: {message}");
            }
            catch
            {
                //at this point we're stuck, so just no-op
            }

            return Task.CompletedTask;
        }
    }
}


/** 

TODO - consider implementing logging library instead 
*/