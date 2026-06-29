using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motwane.UVSS.Domain.Entities
{
    public class DiagnosticStatus
    {
        public string ComponentName { get; set; }

        public bool IsHealthy { get; set; }

        public string Message { get; set; }

        public DateTime CheckedTime { get; set; }
    }
}
