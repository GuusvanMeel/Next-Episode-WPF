using Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Helper
{
    static class ResponseHelper
    {
        public static ResponseBody Check(ResponseBody result)
        {
            if (result.Success == false)
            {
                return ResponseBody.Fail(result.Message ?? "Unknown error");
            }
            return ResponseBody.Ok();
        }
        public static ResponseBody<T> Check<T>(ResponseBody<T> result)
        {
            if (result.Success == false)
            {
                return ResponseBody<T>.Fail(result.Message ?? "Unknown error");
            }
            return ResponseBody<T>.Ok(result.Data!);
        }
    }
}
