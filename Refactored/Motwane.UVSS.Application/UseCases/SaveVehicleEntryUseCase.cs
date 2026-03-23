using Motwane.UVSS.Application.DTOs;
using Motwane.UVSS.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                request.UndersideImage,
                request.DriverImage,
                request.AnprImage
            );

            _vehicleService.SaveVideoRecord(
                request.NumberPlate,
                request.Video1Path,
                request.Video2Path,
                request.Video3Path,
                request.UndersideImage
            );
        }
    }
}
