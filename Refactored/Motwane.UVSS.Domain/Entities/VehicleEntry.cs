using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Threading.Tasks;
namespace Motwane_UVSS.Domain.Entities
{
    public class VehicleEntry
    {
        public int SrNo { get; set; }
        public string Username { get; set; }
        public DateTime? EntryDate { get; set; }
        public TimeSpan? EntryTime { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string Numberplate { get; set; }

        public byte[] UndersideBytes { get; set; }
        public byte[] DriverCamBytes { get; set; }
        public byte[] AnprBytes { get; set; }

      
    }
}