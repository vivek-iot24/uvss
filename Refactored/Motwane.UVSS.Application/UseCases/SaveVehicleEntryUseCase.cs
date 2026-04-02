using Motwane.UVSS.Application.DTOs;
using Motwane.UVSS.Application.Services;
using System;

namespace Motwane.UVSS.Application.UseCases
{
    public class SaveVehicleEntryUseCase
    {
        private readonly VehicleEntryService _vehicleService;

        public SaveVehicleEntryUseCase(VehicleEntryService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        public void Execute(VehicleEntryRequest request)
        {
            var lastRemark = _vehicleService.GetLastVehicleRemark(request.NumberPlate);

            if (lastRemark != null && lastRemark != "Normal")
            {
                throw new InvalidOperationException(
                    $"Vehicle flagged: {lastRemark}"
                );
            }

            _vehicleService.SaveVehicleEntry(
                request.Username,
                request.EntryDate,
                request.EntryTime,
                request.Status,
                request.Remark,
                request.NumberPlate,
                request.UndersideImagePath,
                request.DriverImagePath,
                request.AnprImagePath,
                request.Video1Path,
                request.Video1Path,
                request.Video1Path
            );
        }
    }
}