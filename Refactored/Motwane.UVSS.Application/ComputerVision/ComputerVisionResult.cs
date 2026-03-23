namespace Motwane.UVSS.Application.ComputerVision
{
    /// <summary>
    /// Holds the results of computer vision processing (ANPR, logo/brand detection).
    /// </summary>
    public class ComputerVisionResult
    {
        /// <summary>Path to the cropped number-plate image.</summary>
        public string PlateCropPath { get; set; }

        /// <summary>OCR-recognised registration number.</summary>
        public string RegistrationNumber { get; set; }

        /// <summary>Path to the cropped logo image.</summary>
        public string LogoCropPath { get; set; }

        /// <summary>Detected brand / make name.</summary>
        public string BrandName { get; set; }

        /// <summary>Error message if processing failed; null on success.</summary>
        public string Error { get; set; }
    }
}
