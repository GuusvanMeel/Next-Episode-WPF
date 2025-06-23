using Interfaces.Entities;
using Interfaces.interfaces;
using System.Diagnostics;

namespace Service
{
    public class PlayerService(ILoggerService logger, ISettingsRepo settingsRepo)
    {
        private readonly ILoggerService Logger = logger;

        private readonly ISettingsRepo SettingsRepo = settingsRepo;

        public ResponseBody StartVideo(string videofilepath)
        {
            try
            {
                ResponseBody<AppSettings> settings = SettingsRepo.LoadSettings();
                if (SettingsLoadFailed(settings))
                {
                    return ResponseBody.Fail(settings.Message ?? "Failed to load settings.");
                }
                string playerPath = settings.Data!.VideoPlayerPath;

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
                Logger.LogException(nameof(StartVideo), ex);
                return ResponseBody.Fail("Failed to start the video");
            }
        }
        public ResponseBody SetVideoPlayer(string path)
        {
            try
            {
                var settings = SettingsRepo.LoadSettings();
                if (SettingsLoadFailed(settings))
                {
                    return ResponseBody.Fail(settings.Message ?? "Error loading user settings");
                }
                settings.Data!.VideoPlayerPath = path;
                var result = SettingsRepo.SaveSettings(settings.Data);
                if (result.Success == false)
                {
                    return ResponseBody.Fail(result.Message ?? "erro saving the user settings");
                }
                return ResponseBody.Ok();

            }
            catch (Exception ex)
            {
                Logger.LogException(nameof(SetVideoPlayer), ex);
                return ResponseBody.Fail("Failed to set the video player");
            }
        }
        private bool SettingsLoadFailed(ResponseBody<AppSettings> settings)
        {
            return !settings.Success || settings.Data == null;
        }
    }
}
