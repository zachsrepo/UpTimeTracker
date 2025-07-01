using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpTimeTracker.Models
{
    public class UptimeLog
    {
        public int Id { get; set; }
        public string Url { get; set; } = null!;
        public bool IsUp { get; set; }
        public int StatusCode { get; set; }
        public int ResponseTimeMs { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
