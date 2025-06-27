using Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.interfaces
{
    public interface IActivityLogRepo
    {
        ResponseBody Log(ActivityLog entry); // Save a new entry

        ResponseBody<List<ActivityLog>> GetAll(); // Get all entries

        ResponseBody<List<ActivityLog>> GetRecent(int count); // Get N most recent

        ResponseBody<List<ActivityLog>> GetForShow(string showTitle); // Filtered by show

        ResponseBody DeleteAll(); // Optional: clear log (for debugging or reset)
        ResponseBody<ActivityLog> GetLast();
    }
}
