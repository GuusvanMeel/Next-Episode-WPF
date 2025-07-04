using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.Entities
{
    public class WatchResult
    {
        public required Show UpdatedShow { get; set; }
        public required ActivityLog ActivityLog { get; set; }
    }
}
