using Interfaces.Entities;
using Interfaces.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Helper
{
    static class ExceptionHelper
    {
        public static ResponseBody FailWithLog(ILoggerService logger, string context, Exception ex, string userMessage = "An unexpected error occurred.")
        {
            logger.LogError($"[{context}] Exception: {ex.Message}");
            return ResponseBody.Fail(userMessage);
        }

        public static ResponseBody<T> FailWithLog<T>(ILoggerService logger, string context, Exception ex, string userMessage = "An unexpected error occurred.")
        {
            logger.LogError($"[{context}] Exception: {ex.Message}");
            return ResponseBody<T>.Fail(userMessage);
        }
    }
}
