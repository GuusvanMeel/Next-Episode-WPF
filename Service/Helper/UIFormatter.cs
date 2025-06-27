using Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Helper
{
     public static class UIFormatter
    {
        public static string FormatNextEpisodeInfo(Episode episode)
        {
            var season = episode.Season.ToString("D2");
            var episodeNum = episode.Number.ToString("D2");
            var duration = FormatDuration(episode.Duration);

            return $"Season {season}, Episode {episodeNum}\nDuration: {duration}";
        }
        public static string FormatActivity(ActivityLog log)
        {
                string show = string.IsNullOrWhiteSpace(log.ShowTitle) ? "[Unknown Show]" : log.ShowTitle;
                string when = log.When.ToString("g");

                string message = log.Type switch
                {
                    ActivityLog.ActivityType.WatchedEpisode =>
                        $"Watched {show}" +
                        (log.Season.HasValue && log.Episode.HasValue
                            ? $" S{log.Season:00}E{log.Episode:00}" +
                              (string.IsNullOrWhiteSpace(log.EpisodeTitle) ? "" : $" – \"{log.EpisodeTitle}\"")
                            : "") +
                        $" on {when}.",

                    ActivityLog.ActivityType.ShowFinished =>
                        $"Marked {show} as finished on {when}.",

                    ActivityLog.ActivityType.ResetProgress =>
                        $"Reset progress for {show} on {when}.",

                    ActivityLog.ActivityType.AddedShow =>
                        $"Added show: {show} on {when}.",

                    ActivityLog.ActivityType.EditedShow =>
                        $"Edited show metadata: {show} on {when}.",

                    _ => $"Performed {log.Type} on {show} on {when}."
                };

                return message;
            
        }

        public static string FormatDuration(TimeSpan duration)
        {
            var minutes = (int)duration.TotalMinutes;
            var seconds = duration.Seconds;
            return $"{minutes}m {seconds:D2}s";
        }
    }
}
