using Interfaces.Entities;
using Interfaces.interfaces;
using System.Text.Json;

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
                string[] showFiles = Directory.GetFiles(showsDirectory, "*.json");
                List<string> names = new();

                foreach (string file in showFiles)
                {
                    string json = File.ReadAllText(file);
                    Show? show = JsonSerializer.Deserialize<Show>(json);
                    if (show != null && !string.IsNullOrWhiteSpace(show.Name))
                    {
                        names.Add(show.Name);
                    }
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
                string safeName = GetSafeShowFileName(showname);
                string filePath = Path.Combine(showsDirectory, safeName + ".json");
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
                string safeName = GetSafeShowFileName(show.Name);
                show.PosterFileName = safeName + ".jpg";
                string filePath = Path.Combine(showsDirectory, safeName + ".json");
                string json = JsonSerializer.Serialize(show, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
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
                string safeName = GetSafeShowFileName(showname);
                string filePath = Path.Combine(showsDirectory, safeName + ".json");
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
                string safeName = GetSafeShowFileName(show.Name);
                string filePath = Path.Combine(showsDirectory, safeName + ".json");
                string json = JsonSerializer.Serialize(show, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogException("SaveShow", ex);
                return false;
            }
        }

        public List<string> GetShowNamesInApp()
        {
            try
            {
                string filePath = Path.Combine(AppContext.BaseDirectory, "data", "shownames.json");

                if (!File.Exists(filePath))
                {
                    // Log or handle missing file scenario as needed
                    return new List<string>();
                }

                string json = File.ReadAllText(filePath);

                var names = JsonSerializer.Deserialize<List<string>>(json);

                return names ?? new List<string>();
            }
            catch (Exception ex)
            {
                // Log error appropriately (inject ILoggerService if needed)
                Console.WriteLine($"Failed to load show names: {ex.Message}");
                return new List<string>();
            }
        }
        private string GetSafeShowFileName(string showTitle)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                showTitle = showTitle.Replace(c, '_');
            }
            return showTitle;
        }
    }
}
