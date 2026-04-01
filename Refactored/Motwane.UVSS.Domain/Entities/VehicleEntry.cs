using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Threading.Tasks;

namespace Motwane.UVSS.Domain.Entities
{
    public class VehicleEntry
    {
        public int UserId { get; set; }
        public int MachineId { get; set; }
        public int? GateId { get; set; }

        public string Username { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string Numberplate { get; set; }

        public string UndersideImagePath { get; set; }
        public string DriverImagePath { get; set; }
        public string AnprImagePath { get; set; }

        public string Video1Path { get; set; }
        public string Video2Path { get; set; }
        public string Video3Path { get; set; }
    }
}
