using Motwane.UVSS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motwane.UVSS.Infrastructure.HAL.Interfaces
{
    public interface ISelfDiagnosisService
    {
        Task<List<DiagnosticStatus>> RunDiagnostics();

        Task<DiagnosticStatus> CheckDatabase();

        Task<DiagnosticStatus> CheckStorage();

        Task<DiagnosticStatus> CheckNetwork();

        Task<DiagnosticStatus> CheckAlarm();

        Task<List<DiagnosticStatus>> CheckCameras();
    }
}
