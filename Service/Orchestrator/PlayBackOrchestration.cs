using Interfaces.Entities;
using Interfaces.interfaces;
using Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Orchestrator
{
    public class PlaybackOrchestration
    {
        private readonly UserService userService;
        private readonly PlayerService playerService;
        private readonly EpisodeService episodeService;
        private readonly ActivityService activityService;
        private readonly ILoggerService logger;

        public PlaybackOrchestration(
            UserService userService,
            PlayerService playerService,
            EpisodeService episodeService,
            ActivityService activityService,
            ILoggerService loggerservice)
        {
            this.userService = userService;
            this.playerService = playerService;
            this.episodeService = episodeService;
            this.activityService = activityService;
            this.logger = loggerservice;
        }

        public ResponseBody<WatchResult> WatchNextEpisode(Show currentShow, bool playepisode)
        {
            try
            {
                Episode? currentEpisode = episodeService.FindCurrentEpisode(currentShow);
                if (currentEpisode == null)
                {
                    return ResponseBody<WatchResult>.Fail("No episode available to mark as watched.");
                }

                userService.UpdateEpisodesWatched(1);
                userService.IncreaseTimeWatched(currentEpisode.Duration);
                if (playepisode)
                {
                    var startVideoResult = playerService.StartVideo(currentEpisode.FilePath);
                    if (!ResponseHelper.Check(startVideoResult))
                    {
                        return ResponseBody<WatchResult>.Fail(startVideoResult.Message!);
                    }
                }
                

                var logEpisode = episodeService.LogEpisodeWatched(currentShow);
                if (!ResponseHelper.Check(logEpisode))
                {
                    return ResponseBody<WatchResult>.Fail(logEpisode.Message!);
                }

                if (logEpisode.Data!.IsFinished)
                {
                    userService.UpdateShowsWatched(1);
                }

                var logActivity = activityService.LogEpisodeWatched(
                    currentEpisode.ShowName, currentEpisode.Season, currentEpisode.Number);

                if (!ResponseHelper.Check(logActivity))
                {
                    return ResponseBody<WatchResult>.Fail(logActivity.Message!);
                }

                var result = new WatchResult
                {
                    UpdatedShow = logEpisode.Data,
                    ActivityLog = logActivity.Data!
                };

                return ResponseBody<WatchResult>.Ok(result);
            }
            catch (Exception ex)
            {
                return ExceptionHelper.FailWithLog<WatchResult>(logger,nameof(WatchNextEpisode), ex, "Unexpected error");
            }
        }
    }
}
