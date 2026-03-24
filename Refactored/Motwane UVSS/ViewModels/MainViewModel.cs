using Motwane.UVSS.Application.DTOs;
using Motwane.UVSS.Application.UseCases;
using Motwane.UVSS.HAL;
using Motwane.UVSS.Infrastructure.Infrastructure.HAl.Services;
using Motwane.UVSS.Presentation.Commands;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;


namespace Motwane.UVSS.Presentation.ViewModels
{
    public class MainViewModel
    {
        private readonly SaveVehicleEntryUseCase _saveUseCase;
        private readonly IImageService _imageService;
        private readonly IImageProcessingService _imageProcessing;
       // private readonly ISensorService _sensorService;
        public ICommand PassCommand { get; }
        public ICommand HoldCommand { get; }

        public string Username { get; set; }
        public string Remark { get; set; }
        public string NumberPlate { get; set; }

        public MainViewModel(
     SaveVehicleEntryUseCase saveUseCase,
     IImageService imageService,
     IImageProcessingService imageProcessingService)
        {
            _saveUseCase = saveUseCase;
            _imageService = imageService;
            _imageProcessing = imageProcessingService;

            PassCommand = new RelayCommand(() => Execute("PASS"));
            HoldCommand = new RelayCommand(() => Execute("HOLD"));
        }
        private void Execute(string status)
        {
            var now = System.DateTime.Now;

            var request = new Motwane.UVSS.Application.DTOs.VehicleEntryRequest
            {
                Username = Username,
                Status = status,
                Remark = Remark,
                NumberPlate = NumberPlate,
                EntryDate = now.Date,
                EntryTime = now.TimeOfDay,

                UndersideImage = _imageService.GetUndersideImage(),
                DriverImage = _imageService.GetDriverImage(),
                AnprImage = _imageService.GetAnprImage(),

                Video1Path = _imageService.GetVideoPath(1),
                Video2Path = _imageService.GetVideoPath(2),
                Video3Path = _imageService.GetVideoPath(3)
            };

            _saveUseCase.Execute(request);
        }
    }
}