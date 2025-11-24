using Interfaces.Entities;
using Interfaces.interfaces;
using Service.Helper;
using System.Diagnostics;

namespace Service
{
    public class PlayerService(ILoggerService _logger, ISettingsRepo settingsRepo)
    {
        private readonly ILoggerService logger = _logger;

        private readonly ISettingsRepo SettingsRepo = settingsRepo;

        public ResponseBody StartVideo(string videofilepath)
        {
            try
            {
                ResponseBody<AppSettings> settings = SettingsRepo.LoadSettings();
                if (!ResponseHelper.Check(settings))
                {
                    return ResponseBody.Fail(settings.Message ?? "Failed to load settings.");
                }
                string playerPath = settings.Data!.VideoPlayerPath!;

                if (!File.Exists(videofilepath))
                {
                    return ResponseBody.Fail("Video file no longer exists.");
                }

                if (!File.Exists(playerPath))
                {
                    string playerExe = Path.GetFileName(playerPath);
                    return ResponseBody.Fail($"{playerExe} is missing or invalid.");
                }
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = playerPath,
                        Arguments = $"--fullscreen \"{videofilepath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                return ResponseBody.Ok();
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog(logger, nameof(StartVideo), ex, "Unexpected error");

            }
        }
      

    }
}
