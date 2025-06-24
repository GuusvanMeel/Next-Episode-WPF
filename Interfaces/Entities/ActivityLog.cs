using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.Entities
{
    public class ActivityLog
    {
        public DateTime When { get; set; } = DateTime.Now;

        public string? ShowTitle { get; set; } // e.g. "Breaking Bad"
        public int? Season { get; set; } // Optional
        public int? Episode { get; set; } // Optional
        public string? EpisodeTitle { get; set; } // Optional but nice to have
        public ActivityType Type { get; set; }

        public enum ActivityType
        {
            WatchedEpisode,
            MarkedShowFinished,
            MarkedShowUnfinished,
            ResetProgress,
            DeletedShowMetadata,
            AddedShow,
            EditedShow,
            ChangedRegex,
            ImportedFolder,
            StartedApp
        }
    }
}
