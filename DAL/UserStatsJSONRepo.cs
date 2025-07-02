using Interfaces.Entities;
using Interfaces.interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace DAL
{
    public class UserStatsJSONRepo : IUserStatsRepo
    {
        private readonly string StatsFile = Path.Combine(AppContext.BaseDirectory, "data", "Statistics.json");

        private readonly ILoggerService logger;
        public UserStatsJSONRepo(ILoggerService _logger)
        {
            var settingsDir = Path.GetDirectoryName(StatsFile);
            if (!string.IsNullOrEmpty(settingsDir))
                Directory.CreateDirectory(settingsDir);

            if (!File.Exists(StatsFile))
                File.WriteAllText(StatsFile, "{}"); // create empty JSON file

            this.logger = _logger;
            
        }
        public UserStats GetStats()
        {
            try
            {
                string json = File.ReadAllText(StatsFile);
                var stats = JsonSerializer.Deserialize<UserStats>(json);

                if (stats == null)
                {
                    logger.LogError("In GetStats, Stats file was empty or invalid JSON. Initializing new UserStats.");
                    stats = new UserStats();
                    SaveStats(stats); // ensure file is initialized with default stats
                }

                return stats;
            }
            catch (Exception ex)
            {
                logger.LogException("GetStats", ex);
                throw;
            }
        }
        public bool SaveStats(UserStats stats)
        {
            try
            {
                string json = JsonSerializer.Serialize(stats, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(StatsFile, json);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogException("SaveStats", ex);
                return false;
            }
        }
        

        public bool AddTimeWatched(TimeSpan duration)
        {
            try
            {
                UserStats stats = GetStats();

                stats.TotalTimeWatched = stats.TotalTimeWatched.Add(duration);
                return SaveStats(stats);

            }
            catch (Exception ex)
            {
                logger.LogException("AddTimeWatched", ex);
                return false;
            }
        }

        public List<Episode> GetFavourites()
        {
            try
            {
                UserStats stats = GetStats(); // will throw if null

                return stats.Favourites ??= new List<Episode>();
            }
            catch (Exception ex)
            {
                logger.LogException("GetFavourites", ex);
                return new();
            }
        }

       

        public bool IncrementEpisodesWatched(int amount = 1)
        {
            try
            {
                UserStats stats = GetStats();

                stats.EpsWatched += amount;

                return SaveStats(stats);

            }
            catch (Exception ex)
            {
                logger.LogException("IncrementEpisodesWatched", ex);
                return false;
            }
        }

        public bool IncrementShowsWatched(int amount = 1)
        {
            try
            {
                UserStats stats = GetStats();

                stats.ShowsCompleted += amount;

                return SaveStats(stats);

            }
            catch (Exception ex)
            {
                logger.LogException("IncrementShowsWatched", ex);
                return false;
            }
        }

        public bool AddFavouriteEpisode(Episode episode)
        {
            try
            {
                UserStats stats = GetStats(); // will throw if null

                stats.Favourites ??= new List<Episode>();

                if (!stats.Favourites.Any(e => e.FilePath.Equals(episode.FilePath, StringComparison.OrdinalIgnoreCase)))
                {
                    stats.Favourites.Add(episode);
                }
                return SaveStats(stats);

            }
            catch (Exception ex)
            {
                logger.LogException("AddFavouriteEpisode", ex);
                return false;
            }
        }
        public bool RemoveFavouriteEpisode(Episode episode)
        {
            try
            {
                UserStats stats = GetStats(); // will throw if null

                if (stats.Favourites == null)
                    return true; // nothing to remove, method succeeds

                var existing = stats.Favourites
                               .FirstOrDefault(e => e.FilePath.Equals(episode.FilePath, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    stats.Favourites.Remove(existing);
                }
                return SaveStats(stats);

            }
            catch (Exception ex)
            {
                logger.LogException("RemoveFavouriteEpisode", ex);
                return false;
            }
        }
    }
}
