using Interfaces.Entities;
using Interfaces.interfaces;

namespace Service
{
    public class EpisodeService(IShowRepo Repo, ILoggerService _logger)
    {
        private readonly IShowRepo ShowRepo = Repo;

        private readonly ILoggerService logger = _logger;

        public ResponseBody LogEpisodeWatched(Show show)
        {
            try
            {

                Episode? justwatched = FindCurrentEpisode(show);
                if (justwatched == null)
                {
                    logger.LogInfo($"LogEpisodeWatched: Current episode not found in '{show.Name}'.");
                    return ResponseBody.Fail("Could not find current episode.");
                }

                justwatched.WhenWatched = DateTime.Now;

                Episode? next = GetNextEpisode(show, justwatched);
                show.CurrentEpisodePath = next?.FilePath;

                ShowRepo.SaveShow(show);

                return ResponseBody.Ok();
            }
            catch (Exception ex)
            {
                logger.LogException(nameof(FindCurrentEpisode), ex);

                return ResponseBody.Fail("Unexpected error while logging episode.");
            }
        }

        public ResponseBody SetCurrentEpisode(Episode episode)
        {
            try
            {
                Show? show = ShowRepo.GetShow(episode.ShowName);
                if (show == null)
                {
                    return ResponseBody.Fail("could not find show");
                }
                show.CurrentEpisodePath = episode.FilePath;
                ShowRepo.SaveShow(show);
                return ResponseBody.Ok();
            }
            catch (Exception ex)
            {
                logger.LogException(nameof(SetCurrentEpisode), ex);

                return ResponseBody.Fail("Unexpected error while setting new current episode.");
            }
        }

        public ResponseBody<Episode> GetCurrentEpisode(Show show)
        {
            try
            {

                Episode? current = FindCurrentEpisode(show);
                if (current == null)
                {
                    return ResponseBody<Episode>.Fail("No current episode set or episode not found.");
                }

                return ResponseBody<Episode>.Ok(current);
            }
            catch (Exception ex)
            {
                logger.LogException(nameof(GetCurrentEpisode), ex);

                return ResponseBody<Episode>.Fail("Unexpected error while retrieving current episode.");
            }
        }

        public ResponseBody<Episode> GetNextEpisode(Show show)
        {
            try
            {
                ResponseBody<Episode> currentResult = GetCurrentEpisode(show);
                if (!currentResult.Success)
                    return ResponseBody<Episode>.Fail(currentResult.Message ?? "Current Episode Error");

                Episode? next = GetNextEpisode(show, currentResult.Data!);
                if (next == null)
                {
                    return ResponseBody<Episode>.Fail("No next episode found.");
                }
                return ResponseBody<Episode>.Ok(next);

            }
            catch (Exception ex)
            {
                logger.LogException(nameof(GetNextEpisode), ex);

                return ResponseBody<Episode>.Fail("Unexpected error while retrieving the next episode.");
            }
        }

        public ResponseBody<List<Episode>> GetWatchedEpisodes(Show show)
        {
            try
            {
                List<Episode> episodes = GetAllEpisodes(show)
                    .Where(e => e.WhenWatched != null)
                    .OrderBy(x => x.Number)
                    .ToList();

                return ResponseBody<List<Episode>>.Ok(episodes);
            }
            catch (Exception ex)
            {
                logger.LogException(nameof(GetWatchedEpisodes), ex);

                return ResponseBody<List<Episode>>.Fail("Unexpected error while retrieving watched episodes.");
            }
        }

        public ResponseBody<List<Episode>> GetUnWatchedEpisodes(Show show)
        {
            try
            {
                List<Episode> episodes = GetAllEpisodes(show)
                    .Where(e => e.WhenWatched == null)
                    .OrderBy(x => x.Number)
                    .ToList();

                return ResponseBody<List<Episode>>.Ok(episodes);
            }
            catch (Exception ex)
            {
                logger.LogException(nameof(GetUnWatchedEpisodes), ex);

                return ResponseBody<List<Episode>>.Fail("Unexpected error while retrieving unwatched episodes.");
            }
        }

        public ResponseBody<double> GetWatchProgress(Show show)
        {
            try
            {
                ResponseBody<List<Episode>> watched = GetWatchedEpisodes(show);
                List<Episode> allEpisodes = GetAllEpisodes(show);

                if (!watched.Success || watched.Data == null || allEpisodes.Count == 0)
                {
                    return ResponseBody<double>.Fail("Unable to calculate progress: no data.");
                }

                double percentage = (double)watched.Data.Count / allEpisodes.Count * 100;
                return ResponseBody<double>.Ok(percentage);
            }
            catch (Exception ex)
            {
                logger.LogException(nameof(GetWatchProgress), ex);
                return ResponseBody<double>.Fail("Unexpected error while calculating progress.");
            }
        }

        private static Episode? FindCurrentEpisode(Show show)
        {
            return GetAllEpisodes(show)
                .FirstOrDefault(e => e.FilePath == show.CurrentEpisodePath);
        }

        private static Episode? GetNextEpisode(Show show, Episode current)
        {
            List<Episode> allEpisodes = GetAllEpisodes(show);

            int currentIndex = allEpisodes.FindIndex(e => e.FilePath == current.FilePath);
            return (currentIndex + 1 < allEpisodes.Count) ? allEpisodes[currentIndex + 1] : null;
        }

        private static List<Episode> GetAllEpisodes(Show show)
        {
            return show.Seasons.OrderBy(s => s.Number).SelectMany(s => s.Episodes.OrderBy(x => x.Number)).ToList();
        }
    }
}
