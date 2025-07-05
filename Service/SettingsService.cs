using Interfaces.Entities;
using Interfaces.interfaces;
using Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class SettingsService
    {
        private readonly ILoggerService logger;
        private readonly ISettingsRepo SettingsRepo;
        public SettingsService(ISettingsRepo Srepo, ILoggerService _logger)
        {
            this.SettingsRepo = Srepo;
            this.logger = _logger;
        }
        public ResponseBody SetVideoPlayer(string path)
        {
            try
            {
                var settings = SettingsRepo.LoadSettings();
                if (!ResponseHelper.Check(settings))
                {
                    return ResponseBody.Fail(settings.Message ?? "Error loading user settings");
                }
                settings.Data!.VideoPlayerPath = path;
                var result = SettingsRepo.SaveSettings(settings.Data);
                if (result.Success == false)
                {
                    return ResponseBody.Fail(result.Message ?? "error saving the user settings");
                }
                return ResponseBody.Ok();

            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog(logger, nameof(SetVideoPlayer), ex, "Unexpected error");
            }
        }
        public ResponseBody<AppSettings> GetCurrentSettings()
        {
            var result = SettingsRepo.LoadSettings();
            if (!ResponseHelper.Check(result))
            {
                return ResponseBody<AppSettings>.Fail(result.Message ?? "Error loading user settings");
            }
            return result;
        }
        
    }
}
