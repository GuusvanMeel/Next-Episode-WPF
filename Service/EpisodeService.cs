using Interfaces.Entities;
using Interfaces.interfaces;
using Service.Helper;

namespace Service
{
    public class EpisodeService(IShowRepo Repo, ILoggerService _logger)
    {
        private readonly IShowRepo ShowRepo = Repo;

        private readonly ILoggerService logger = _logger;

        public ResponseBody<Show> LogEpisodeWatched(Show show)
        {
            try
            {

                Episode? justwatched = FindCurrentEpisode(show);
                if (justwatched == null)
                {
                    logger.LogInfo($"LogEpisodeWatched: Current episode not found in '{show.Name}'.");
                    return ResponseBody<Show>.Fail("Could not find current episode.");
                }

                justwatched.WhenWatched = DateTime.Now;

                Episode? next = GetNextEpisode(show, justwatched);
                if (next == null)
                {
                    show.IsFinished = true;
                    logger.LogInfo($"LogEpisodeWatched: '{show.Name}' marked as finished.");
                    ShowRepo.SaveShow(show);
                    return ResponseBody<Show>.Ok(show);
                }
                show.CurrentEpisodePath = next.FilePath;

                ShowRepo.SaveShow(show);

                return ResponseBody<Show>.Ok(show);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog<Show>(logger, nameof(LogEpisodeWatched), ex, "Unexpected error");
            }
        }

        public ResponseBody<Show> SetCurrentEpisode(Episode episode)
        {
            try
            {
                Show? show = ShowRepo.GetShow(episode.ShowName);
                if (show == null)
                {
                    return ResponseBody<Show>.Fail("Could not find show.");
                }

                // Sort all episodes in proper order
                var allEpisodes = show.Seasons
                    .SelectMany(s => s.Episodes)
                    .OrderBy(e => e.Season)
                    .ThenBy(e => e.Number)
                    .ToList();

                foreach (var ep in allEpisodes)
                {
                    if (string.Equals(ep.FilePath, episode.FilePath, StringComparison.OrdinalIgnoreCase))
                    {
                        // Found current episode
                        ep.WhenWatched = null;
                    }
                    else if (ep.Season < episode.Season || (ep.Season == episode.Season && ep.Number < episode.Number))
                    {
                        // Episodes before current are marked as watched
                        ep.WhenWatched ??= DateTime.Now;
                    }
                    else
                    {
                        // Episodes after current are unwatched
                        ep.WhenWatched = null;
                    }
                }

                show.CurrentEpisodePath = episode.FilePath;
                logger.LogInfo($"SetCurrentEpisode: '{episode.FilePath}' set as current for show '{episode.ShowName}'.");

                ShowRepo.SaveShow(show);

                return ResponseBody<Show>.Ok(show);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog<Show>(logger, nameof(SetCurrentEpisode), ex, "Unexpected error");
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
                return ExceptionHelper.FailWithLog<Episode>(logger, nameof(GetCurrentEpisode), ex, "Unexpected error");
            }
        }

        public ResponseBody<Episode> GetNextEpisode(Show show)
        {
            try
            {
                var currentResult = GetCurrentEpisode(show);
                if (!ResponseHelper.Check(currentResult))
                {
                    return ResponseBody<Episode>.Fail(currentResult.Message!);
                }

                Episode? next = GetNextEpisode(show, currentResult.Data!);
                if (next == null)
                {
                    return ResponseBody<Episode>.Fail("No next episode found.");
                }
                return ResponseBody<Episode>.Ok(next);

            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog<Episode>(logger, nameof(GetNextEpisode), ex, "Unexpected error");
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
                return ExceptionHelper.FailWithLog<List<Episode>>(logger, nameof(GetWatchedEpisodes), ex, "Unexpected error");
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

                return ExceptionHelper.FailWithLog<List<Episode>>(logger, nameof(GetUnWatchedEpisodes), ex, "Unexpected error");
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

        public Episode? FindCurrentEpisode(Show show)
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
