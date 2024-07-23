using System.Collections;
using System.Diagnostics;
using Goddard.Clock.Data;

namespace Goddard.Clock.Helpers;
public class Logging
{
    public async static Task Log(ClockDatabase? _database, Exception ex, string message = "", string? parentExceptionID = null)
    {
        try
        {
            if (ex == null)
                return;

            DebugWrite(ex, message, parentExceptionID);

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
                HelpLink = ex?.HelpLink ?? "",
                HResult = ex?.HResult ?? 0,
                InnerExceptionID = parentExceptionID ?? "",
                ExceptionMessage = ex?.Message ?? "",
                Source = ex?.Source ?? "",
                StackTrace = ex?.StackTrace ?? "",
                OptionalMessage = message,
                ExceptionData = dict
            };

            if(_database != null)
                _ = await _database.LocalLogInsert(log);

            //if inner except, recursively log it too
            if (ex?.InnerException != null)
                _ = Log(_database, ex.InnerException, message, log.GloballyUniqueID);

            if (ex is AggregateException agg)
            {
                foreach (var inner in agg.InnerExceptions)
                    _ = Log(_database, inner, message, log.GloballyUniqueID);


                agg.Handle(ae =>
                {
                    _ = Log(_database, ae, message, log.GloballyUniqueID);
                    return true;
                });
            }
        }
        catch (Exception logException)
        {
            DebugWrite(logException, "Exception during Goddard.Clock.Helpers.Logging.Log(exception, string, string)", parentExceptionID);
        }

    }

    public async static Task Log(ClockDatabase? _database, string message)
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
            if(_database != null)
                _ = await _database.LocalLogInsert(log);
        }
        catch (Exception logException)
        {
            DebugWrite(logException, "Exception during Goddard.Clock.Helpers.Logging.Log(string)");
        }
    }

    public static void DebugWrite(Exception ex, string message = "", string? parentExceptionID = null)
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