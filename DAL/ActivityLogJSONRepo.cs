using Interfaces.Entities;
using Interfaces.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DAL
{
    internal class ActivityLogJSONRepo : IActivityLogRepo
    {
        private readonly string Logfile = Path.Combine(AppContext.BaseDirectory, "data", "ActivityLog.json");

        private readonly ILoggerService logger;

        public ActivityLogJSONRepo(ILoggerService logger)
        {
            var settingsDir = Path.GetDirectoryName(Logfile);
            if (!string.IsNullOrEmpty(settingsDir))
                Directory.CreateDirectory(settingsDir);

            if (!File.Exists(Logfile))
                File.WriteAllText(Logfile, "{}"); // create empty JSON file
            this.logger = logger;
        }
        public ResponseBody DeleteAll()
        {
            throw new NotImplementedException();
        }

        public ResponseBody<List<ActivityLog>> GetAll()
        {
            try
            {
                List<ActivityLog> logs = new();
                if (File.Exists(Logfile))
                {
                    string existingJson = File.ReadAllText(Logfile);
                    logs = JsonSerializer.Deserialize<List<ActivityLog>>(existingJson) ?? new List<ActivityLog>();
                }
              return ResponseBody<List<ActivityLog>>.Ok(logs);
            }
            catch(Exception ex)
            {
                logger.LogError("Failed to get all activity logs: " + ex.Message);
                return ResponseBody<List<ActivityLog>>.Fail("Could not Get all activitylogs.");
            }
        }

        public ResponseBody<List<ActivityLog>> GetForShow(string showTitle)
        {
            try
            {


                List<ActivityLog> logs = new();
                var result = GetAll();
                if (result.Success == false)
                {
                    return ResponseBody<List<ActivityLog>>.Fail(result.Message ?? "Could not get all activity logs.");
                }
                foreach (var r in result.Data!)
                {
                    if (r.ShowTitle == showTitle)
                    {
                        logs.Add(r);
                    }
                }
                return ResponseBody<List<ActivityLog>>.Ok(logs);
            }
            catch(Exception ex)
            {
                logger.LogError($"Failed to get all activity logs for{showTitle}: " + ex.Message);
                return ResponseBody<List<ActivityLog>>.Fail($"Could not Get all activitylogs for {showTitle}.");
            }

        }

        public ResponseBody<List<ActivityLog>> GetRecent(int count)
        {
            try
            {
                
                var result = GetAll();

                if (result.Success == false)
                {
                    return ResponseBody<List<ActivityLog>>.Fail(result.Message ?? "Could not get all activity logs.");
                }
                List <ActivityLog> logs = result.Data!.OrderByDescending(n => n.When).Take(count).ToList();
                
                return ResponseBody<List<ActivityLog>>.Ok(logs);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to get top {count} activitylogs: " + ex.Message);
                return ResponseBody<List<ActivityLog>>.Fail($"Could not get top {count} activitylogs.");
            }
        }

        public ResponseBody Log(ActivityLog entry)
        {
            try
            {

                // Step 1: Load existing log
                var result = GetAll();
                if(result.Success == false)
                {
                    return ResponseBody.Fail(result.Message ?? "Could not get all activity logs.");
                }

                // Step 2: Add the new entry
                result.Data!.Add(entry);

                // Step 3: Save updated log back to file
                string updatedJson = JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Logfile, updatedJson);

                return ResponseBody.Ok();
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to write activity log: " + ex.Message);
                return ResponseBody.Fail("Could not write activity log.");
            }
        }
    }
}
