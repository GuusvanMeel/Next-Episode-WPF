using Interfaces.Entities;

namespace Interfaces.interfaces
{
    public interface ISettingsRepo
    {
        ResponseBody<AppSettings> LoadSettings();
        ResponseBody SaveSettings(AppSettings settings);
    }
}
