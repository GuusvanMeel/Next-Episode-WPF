using Interfaces.Entities;
using Interfaces.interfaces;
using System.Text.Json;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DAL
{
    public class ShowJSONRepo : IShowRepo
    {
        private readonly string showsDirectory = Path.Combine(AppContext.BaseDirectory, "data", "shows");
        private readonly ILoggerService logger;

        public ShowJSONRepo(ILoggerService logger)
        {
            if (!Directory.Exists(showsDirectory))
            {
                Directory.CreateDirectory(showsDirectory);
            }
            this.logger = logger;
        }
        public List<Show> LoadAllShows()
        {
            try
            {
                string[] jsonshows = Directory.GetFiles(showsDirectory, "*.json");
                List<Show> shows = new();
                foreach (string s in jsonshows)
                {
                    try
                    {
                        string json = File.ReadAllText(s);
                        Show? show = JsonSerializer.Deserialize<Show>(json);
                        if (show != null)
                        {
                            shows.Add(show);
                        }
                        else
                        {
                            logger.LogError($"LoadAllShows: Null show deserialized from file '{s}'");
                        }
                    }
                    catch (Exception fileEx)
                    {
                        logger.LogException($"LoadAllShows: Error loading show from '{s}'", fileEx);
                    }
                }
                return shows;
            }
            catch (Exception ex)
            {
                logger.LogException("LoadAllShows", ex);
                return new();
            }
        }
        public List<string> GetAllShowNames()
        {
            try
            {
                string[] shows = Directory.GetFiles(showsDirectory);
                List<string> names = new();
                foreach (string show in shows)
                {
                    names.Add(Path.GetFileNameWithoutExtension(show));
                }
                return names;
            }
            catch (Exception ex)
            {
                logger.LogException("GetAllShowNames", ex);
                return new();
            }
        }
        public Show? GetShow(string showname)
        {

            try
            {
                string filePath = Path.Combine(showsDirectory, showname + ".json");
                string json = File.ReadAllText(filePath);
                Show? show = JsonSerializer.Deserialize<Show>(json);
                if (show == null)
                {
                    throw new Exception("Deserialization failed: JSON was empty or 'null'.");
                }
                return show;
            }
            catch (Exception ex)
            {
                logger.LogException("GetShow", ex);
                return null;
            }
        }

        public bool AddShow(Show show)
        {
            try
            {
                string filepath = Path.Combine(showsDirectory, show.Name + ".json");
                string json = JsonSerializer.Serialize(show, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filepath, json);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogException("AddShow", ex);
            }

            return false;
        }

        public bool DeleteShow(string showname)
        {
            try
            {
                string filePath = Path.Combine(showsDirectory, showname + ".json");
                File.Delete(filePath);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogException("DeleteShow", ex);
                return false;
            }

        }

        public bool AddSeason(Season season, string showname)
        {
            try
            {
                Show? show = GetShow(showname);
                if (show != null)
                {
                    show.Seasons.Add(season);
                    SaveShow(show);
                }
                return false;

            }
            catch (Exception ex)
            {
                logger.LogException("AddSeason", ex);
                return false;
            }

        }

        public bool DeleteSeason(int number, string showname)
        {
            try
            {
                Show? show = GetShow(showname);
                if (show != null)
                {
                    int removed = show.Seasons.RemoveAll(s => s.Number == number);
                    if (removed == 0)
                    {
                        logger.LogInfo($"DeleteSeason: No matching season {number} in '{showname}'");
                        return false;
                    }
                    SaveShow(show);
                    return true;
                }
                else return false;

            }
            catch (Exception ex)
            {
                logger.LogException("DeleteSeason", ex);
                return false;
            }
        }
        public bool SaveShow(Show show)
        {
            try
            {
                string path = Path.Combine(showsDirectory + show.Name + ".json");
                string json = JsonSerializer.Serialize(show, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogException("SaveShow", ex);
                return false;
            }
        }
    }
}
