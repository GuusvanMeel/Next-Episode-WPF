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
        public static bool Check(ResponseBody result)
        {
            if (result.Success == false)
            {
                return false;
            }
            return true;
        }
        public static bool Check<T>(ResponseBody<T> result)
        {
            if (result.Success == false || result.Data == null)
            {
                return false;
            }
            return true;
        }
    }
}
