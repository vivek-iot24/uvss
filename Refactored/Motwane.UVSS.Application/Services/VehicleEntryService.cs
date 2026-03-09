using Motwane_UVSS.Application.Interfaces.DAL;
using Motwane_UVSS.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Motwane_UVSS.Application.Services
{
    public class VehicleEntryService
    {
        private readonly IVehicleEntryRepository _repository;
       // private readonly string _connectionString;

        public VehicleEntryService(
            IVehicleEntryRepository repository)        {
            _repository = repository;
           // _connectionString = connectionString;
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
            byte[] undersideImage,
            byte[] driverCamImage,
            byte[] anprImage)
        {
            _repository.InsertVehicleEntry(
               
                username,
                entryDate,
                entryTime,
                status,
                remark,
                numberplate,
                undersideImage,
                driverCamImage,
                anprImage);
        }

        public void SaveVideoRecord(
            string vehicleNumber,
            string video1,
            string video2,
            string video3,
            byte[] vehicleImage)
        {
            _repository.InsertVideoManagementRecord(
              
                vehicleNumber,
                video1,
                video2,
                video3,
                vehicleImage);
        }
    }
}