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

        void InsertVehicleEntry(
      string username,
      DateTime entryDate,
      DateTime entryTime,
      string status,
      string remark,
      string numberplate,
      string undersideImagePath,
      string driverImagePath,
      string anprImagePath,
      string videoCam1,
      string videoCam2,
      string videoCam3);


    }
}