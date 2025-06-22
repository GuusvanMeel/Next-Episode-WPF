using Interfaces.Entities;
using Interfaces.interfaces;
using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;

namespace Service
{
    public class EpisodeService
    {
        private IShowRepo ShowRepo;
        private ILoggerService logger;
        static string noshow = "could not find show";
        public EpisodeService(IShowRepo _repo, ILoggerService _logger)
        {
            this.ShowRepo = _repo;
            this.logger = _logger;
        }
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
                logger.LogException("LogEpisodeWatched", ex);
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
                    return ResponseBody.Fail(noshow);
                }
                show.CurrentEpisodePath = episode.FilePath;
                ShowRepo.SaveShow(show);
                return ResponseBody.Ok();
            }
            catch (Exception ex)
            {
                logger.LogException("SetCurrentEpisode", ex);
                return ResponseBody.Fail("Unexpected error while setting new current episode.");
            }
        }
        public ResponseBody<Episode> GetCurrentEpisode(Show show)
        {
            try
            {

                Episode? current = FindCurrentEpisode(show);
                if (current == null)
                    return ResponseBody<Episode>.Fail("No current episode set or episode not found.");

                return ResponseBody<Episode>.Ok(current);
            }
            catch (Exception ex)
            {
                logger.LogException("GetCurrentEpisode", ex);
                return ResponseBody<Episode>.Fail("Unexpected error while retrieving current episode.");
            }
        }
        public ResponseBody<Episode> GetNextEpisode(Show show)
        {
            try
            {
                ResponseBody<Episode> currentResult = GetCurrentEpisode(show);
                if (!currentResult.Success)
                    return ResponseBody<Episode>.Fail(currentResult.Message!);

                Episode? next = GetNextEpisode(show, currentResult.Data!);
                if (next == null)
                {
                    return ResponseBody<Episode>.Fail("No next episode found.");
                }
                return ResponseBody<Episode>.Ok(next);

            }
            catch (Exception ex)
            {
                logger.LogException("GetNextEpisode", ex);
                return ResponseBody<Episode>.Fail("Unexpected error while retrieving the next episode.");
            }
        }
        public ResponseBody<List<Episode>> GetWatchedEpisodes(Show show)
        {
            try
            {
                List<Episode> episodes = show.Seasons
                    .SelectMany(s => s.Episodes)
                    .Where(e => e.WhenWatched != null)
                    .ToList();

                return ResponseBody<List<Episode>>.Ok(episodes);
            }
            catch (Exception ex)
            {
                logger.LogException("GetWatchedEpisodes", ex);
                return ResponseBody<List<Episode>>.Fail("Unexpected error while retrieving watched episodes.");
            }

        }
        public ResponseBody<double> GetWatchProgress(Show show)
        {
            GetWatchedEpisodes(Show show);
        }
        private Episode? FindCurrentEpisode(Show show)
        {
            return show.Seasons
                .SelectMany(s => s.Episodes)
                .FirstOrDefault(e => e.FilePath == show.CurrentEpisodePath);
        }
        private Episode? GetNextEpisode(Show show, Episode current)
        {
            var allEpisodes = show.Seasons
                .OrderBy(s => s.Number)
                .SelectMany(s => s.Episodes.OrderBy(e => e.Number))
                .ToList();

            int currentIndex = allEpisodes.FindIndex(e => e.FilePath == current.FilePath);
            return (currentIndex + 1 < allEpisodes.Count) ? allEpisodes[currentIndex + 1] : null;
        }

    }
}
