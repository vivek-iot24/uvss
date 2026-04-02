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

        public void SaveVehicleEntry(
     string username,
     DateTime entryDate,
     TimeSpan entryTime,
     string status,
     string remark,
     string numberplate,
     string undersideImagePath,
     string driverImagePath,
     string anprImagePath,
     string videoCam1,
     string videoCam2,
     string videoCam3)
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
                anprImagePath,
                videoCam1,
                videoCam2,
                videoCam3);
        }
    }
}