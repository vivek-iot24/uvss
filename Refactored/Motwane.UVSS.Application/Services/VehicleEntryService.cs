using Motwane.UVSS.Application.DTOs;
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

        // ✅ MUST RETURN ENTRY ID
        public int SaveVehicleEntry(
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
            return _repository.InsertVehicleEntry(
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

        // ✅ NOW TAKES entryId
        public void SaveVideoRecord(
            int entryId,
            string vehicleNumber,
            string video1,
            string video2,
            string video3,
            string vehicleImagePath)
        {
            _repository.InsertVideoManagementRecord(
                entryId,
                vehicleNumber,
                video1,
                video2,
                video3,
                vehicleImagePath
            );
        }

        // ✅ THIS FIXES YOUR MAIN ERROR
        public void SaveCompleteVehicleLog(VehicleEntryRequest request)
        {
            int entryId = SaveVehicleEntry(
                request.Username,
                request.EntryDate,
                request.EntryTime,
                request.Status,
                request.Remark,
                request.NumberPlate,
                request.UndersideImagePath,
                request.DriverImagePath,
                request.AnprImagePath
            );

            SaveVideoRecord(
                entryId,
                request.NumberPlate,
                request.Video1Path,
                request.Video2Path,
                request.Video3Path,
                request.UndersideImagePath
            );
        }
    }
}