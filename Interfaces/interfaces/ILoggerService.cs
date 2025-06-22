using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.interfaces
{
    public interface ILoggerService
    {
        void LogInfo(string message);
        void LogError(string message);
        void LogException(string context, Exception ex);
    }
}
