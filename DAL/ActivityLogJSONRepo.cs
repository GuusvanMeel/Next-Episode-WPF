using Interfaces.Entities;
using Interfaces.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    internal class ActivityLogJSONRepo : IActivityLogRepo
    {
        private readonly string Logdirectory = Path.Combine(AppContext.BaseDirectory, "data", "shows");

        private readonly ILoggerService logger;

        public ActivityLogJSONRepo(ILoggerService logger)
        {
            if (!Directory.Exists(Logdirectory))
            {
                Directory.CreateDirectory(Logdirectory);
            }
            this.logger = logger;
        }
        public ResponseBody DeleteAll()
        {
            throw new NotImplementedException();
        }

        public ResponseBody<List<ActivityLog>> GetAll()
        {
            throw new NotImplementedException();
        }

        public ResponseBody<List<ActivityLog>> GetForShow(string showTitle)
        {
            throw new NotImplementedException();
        }

        public ResponseBody<List<ActivityLog>> GetRecent(int count)
        {
            throw new NotImplementedException();
        }

        public ResponseBody Log(ActivityLog entry)
        {
            throw new NotImplementedException();
        }
    }
}
