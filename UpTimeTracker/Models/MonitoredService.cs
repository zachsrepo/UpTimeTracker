using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpTimeTracker.Models
{
    public class MonitoredService
    {
        public int Id { get; set; }
        public string Url { get; set; } = null!;
        public string? Name { get; set; }
        public bool LastKnownStatus { get; set; }
        public DateTime? LastChecked { get; set; }
    }

}
