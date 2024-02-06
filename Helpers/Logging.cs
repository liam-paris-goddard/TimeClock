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
        public async static void Log(Exception ex, string message = "", string parentExceptionID = null)
        {
            try
            {
                if (ex == null)
                    return;

                DebugWrite(ex, message, parentExceptionID);

                var dict = "";
                if (ex.Data != null && ex.Data.Count > 0)
                {
                    foreach (DictionaryEntry de in ex.Data)
                        dict += String.Format("    Key: {0,-20}      Value: {1}\n", "'" + de.Key.ToString() + "'", de.Value);
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
                if (ex.InnerException != null)
                    Log(ex.InnerException, message, log.GloballyUniqueID);

                if (ex is AggregateException)
                {
                    var agg = ex as AggregateException;
                    foreach (var inner in agg.InnerExceptions)
                        Log(inner, message, log.GloballyUniqueID);


                    (ex as AggregateException).Handle(ae =>
                    {
                        Log(ae, message, log.GloballyUniqueID);
                        return true;
                    });
                }
            }
            catch (Exception logException)
            {
                DebugWrite(logException, "Exception during TimeClock.Helpers.Logging.Log(exception, string, string)", parentExceptionID);
            }

        }

        public async static void Log(string message)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(message))
                    return;

                DebugWrite(message);

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
                DebugWrite(logException, "Exception during Goddard.Clock.Helpers.Logging.Log(string)");
            }
        }

        public static async void DebugWrite(Exception ex, string message = "", string parentExceptionID = null)
        {
            try
            {
                if (ex == null)
                    return;

                Debug.WriteLine(String.Format("EXCEPTION: {0}\n{1}\n{2}\n{3}", ex.Message, message, ex.Source, ex.StackTrace));
            }
            catch
            {
                //at this point we're stuck, so just no-op
            }
        }

        public static async void DebugWrite(string message)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(message))
                    return;

                Debug.WriteLine(String.Format("MESSAGE: {0}", message));
            }
            catch
            {
                //at this point we're stuck, so just no-op
            }
        }
    }
}
