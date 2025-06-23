using Interfaces.interfaces;

namespace DAL
{
    public class FileLogger : ILoggerService
    {
        private readonly string logFilePath = Path.Combine(AppContext.BaseDirectory, "logs", "log.txt");

        public FileLogger()
        {   
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)!);
        }

        public void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public void LogError(string message)
        {
            Log("ERROR", message);
        }

        private void Log(string level, string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logLine = $"[{timestamp}] [{level}] {message}";
            File.AppendAllText(logFilePath, logLine + Environment.NewLine);
        }

        public void LogException(string context, Exception ex)
        {
            string type = ex.GetType().Name;
            string message = $"{context}: {type} — {ex.Message}";
            LogError(message);
        }
    }

}
