using Motwane.UVSS.Application.Interfaces.DAL;
using Motwane.UVSS.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Motwane.UVSS.Application.Services
{
    public class VehicleEntryService
    {
        private readonly IVehicleEntryRepository _repository;

        public VehicleEntryService(IVehicleEntryRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<string> GetDistinctUsernames()
        {
            return _repository.GetDistinctUsernames();
        }

        public IEnumerable<VehicleEntry> GetEntries(
            DateTime? from,
            DateTime? to,
            string user,
            string plate)
        {
            return _repository.GetVehicleEntryLogs(from, to, user, plate);
        }

        public string GetLastVehicleRemark(string numberplate)
        {
            return _repository.GetLastVehicleRemark(numberplate);
        }

        // ✅ UPDATED: IMAGE PATHS INSTEAD OF BYTE[]
        public void SaveVehicleEntry(
            string username,
            DateTime entryDate,
            TimeSpan entryTime,
            string status,
            string remark,
            string numberplate,
            string undersideImagePath,
            string driverImagePath,
            string anprImagePath)
        {
            _repository.InsertVehicleEntry(
                username,
                entryDate,
                entryTime,
                status,
                remark,
                numberplate,
                undersideImagePath,
                driverImagePath,
                anprImagePath
            );
        }

        // ✅ UPDATED: IMAGE PATH INSTEAD OF BYTE[]
        public void SaveVideoRecord(
            string vehicleNumber,
            string video1,
            string video2,
            string video3,
            string vehicleImagePath)
        {
            _repository.InsertVideoManagementRecord(
                vehicleNumber,
                video1,
                video2,
                video3,
                vehicleImagePath
            );
        }
    }
}