using System;
using System.IO;

namespace JsonGui
{
    public class ErrorLogger
    {
        private static readonly string logFilePath = "application_errors.txt";

        public static void LogErrorToFile(Exception ex, String logFilePath)
        {
            // Get the full details of the exception, including stack trace and inner exceptions
            string logMessage = $"{DateTime.Now}: An error occurred: {ex.ToString()}{Environment.NewLine}";

            try
            {
                // Append the log message to the file. This creates the file if it doesn't exist.
                File.AppendAllText(logFilePath, logMessage);
            }
            catch (Exception loggingEx)
            {
                // Handle exceptions that might occur during the logging process itself (e.g., file access issues)
                Console.WriteLine($"ERROR: Failed to write to log file: {loggingEx.Message}");
            }
        }
    }
}