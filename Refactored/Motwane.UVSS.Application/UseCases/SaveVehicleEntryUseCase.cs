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

            if (!string.IsNullOrWhiteSpace(lastRemark) &&
                !lastRemark.Equals("Normal", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Vehicle flagged: {lastRemark}"
                );
            }

            _vehicleService.SaveCompleteVehicleLog(request);
        }
    }
}