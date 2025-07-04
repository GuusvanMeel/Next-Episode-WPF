﻿namespace Interfaces.Entities
{
    public class ResponseBody
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public static ResponseBody Fail(string message)
        {
            return new ResponseBody
            {
                Success = false,
                Message = message
            };
        }
        public static ResponseBody Ok()
        {
            return new ResponseBody
            {
                Success = true
            };
        }
    }
}
