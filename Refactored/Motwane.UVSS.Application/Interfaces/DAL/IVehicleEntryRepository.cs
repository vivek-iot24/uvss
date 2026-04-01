using Motwane.UVSS.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Motwane.UVSS.Application.Interfaces.DAL
{
    public interface IVehicleEntryRepository
    {
        int GetTotalRowCount(
            DateTime? from,
            DateTime? to,
            string user,
            string plate);

        List<string> GetDistinctUsernames();

        IEnumerable<VehicleEntry> GetVehicleEntryLogs(
            DateTime? from,
            DateTime? to,
            string user,
            string plate);

        string GetLastVehicleRemark(string numberplate);

        // ✅ MUST RETURN ENTRY ID
        int InsertVehicleEntry(
            string username,
            DateTime entryDate,
            TimeSpan entryTime,
            string status,
            string remark,
            string numberplate,
            string undersideImagePath,
            string driverImagePath,
            string anprImagePath
        );

        // ✅ MUST TAKE entryId
        void InsertVideoManagementRecord(
            int entryId,
            string vehicleNumber,
            string video1,
            string video2,
            string video3,
            string vehicleImagePath
        );
    }
}