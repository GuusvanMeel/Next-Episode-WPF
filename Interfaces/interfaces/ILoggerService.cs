namespace Interfaces.interfaces
{
    public interface ILoggerService
    {
        void LogInfo(string message);
        void LogError(string message);
        void LogException(string context, Exception ex);
    }
}
