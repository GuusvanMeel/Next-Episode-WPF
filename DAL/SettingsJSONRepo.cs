using Interfaces.Entities;
using Interfaces.interfaces;
using System.Text.Json;

namespace DAL
{
    public class SettingsJSONRepo : ISettingsRepo
    {
        private readonly ILoggerService Logger;

        private readonly string settingsfile = Path.Combine(AppContext.BaseDirectory, "data", "settings.json");

        public SettingsJSONRepo(ILoggerService logger)
        {
            var settingsDir = Path.GetDirectoryName(settingsfile);
            if (!string.IsNullOrEmpty(settingsDir))
                Directory.CreateDirectory(settingsDir);

            if (!File.Exists(settingsfile))
                File.WriteAllText(settingsfile, "{}"); // create empty JSON file

            this.Logger = logger;
        }

        public ResponseBody<AppSettings> LoadSettings()
        {
            try
            {
                var json = File.ReadAllText(settingsfile);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Logger.LogInfo("Settings file is empty, returning default settings.");
                    return ResponseBody<AppSettings>.Ok(new AppSettings());
                }
                AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json);

                if (settings == null)
                {
                    Logger.LogInfo("Settings file is empty or malformed");
                    return ResponseBody<AppSettings>.Fail("Couldnt find user settings");
                }

                return ResponseBody<AppSettings>.Ok(settings);
            }
            catch (Exception ex)
            {
                Logger.LogException(nameof(LoadSettings), ex);
                return ResponseBody<AppSettings>.Fail("Failed to load user settings");
            }
        }

        public ResponseBody SaveSettings(AppSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsfile, json);
                return ResponseBody.Ok();
            }
            catch (Exception ex)
            {
                Logger.LogException(nameof(SaveSettings), ex);
                return ResponseBody.Fail("Failed to save user settings");
            }
        }
    }
}
