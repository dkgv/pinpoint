using System;
using System.IO;
using System.Threading.Tasks;

namespace Pinpoint.Core
{
    public static class ErrorLogging
    {
        private static readonly string ErrorLogFilePath = AppConstants.MainDirectory + "errorlog.txt";

        public static async Task LogException(Exception e)
        {
            if (!File.Exists(ErrorLogFilePath))
            {
                File.Create(ErrorLogFilePath);
            }

            await using var streamWriter = new StreamWriter(ErrorLogFilePath, true);

            var exception = e;

            await streamWriter.WriteLineAsync($"Timestamp: {DateTime.Now}");

            while (exception != null)
            {
                await streamWriter.WriteLineAsync($"{e.GetType().FullName}:");
                await streamWriter.WriteLineAsync($"Exception message: {e.Message}");
                await streamWriter.WriteLineAsync($"Exception stacktrace: {e.StackTrace}");

                exception = exception.InnerException;
            }
        }
    }
}