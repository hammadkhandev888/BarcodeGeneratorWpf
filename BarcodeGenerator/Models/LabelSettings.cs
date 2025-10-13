using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BarcodeGenerator.Models
{
    /// <summary>
    /// UI-only model representing label dimensions and barcode size settings (NOT a database entity)
    /// </summary>
    public partial class LabelSettings : ObservableObject
    {
        private double _labelWidth = 100.0; // Default 100mm
        private double _labelHeight = 50.0; // Default 50mm
        private double _barcodeWidth = 80.0; // Default 80mm
        private double _barcodeHeight = 20.0; // Default 20mm

        /// <summary>
        /// Label width in millimeters
        /// </summary>
        [Range(10.0, 300.0, ErrorMessage = "Label width must be between 10mm and 300mm")]
        public double LabelWidth
        {
            get => _labelWidth;
            set => _labelWidth = Math.Max(10.0, Math.Min(300.0, value));
        }

        /// <summary>
        /// Label height in millimeters
        /// </summary>
        [Range(10.0, 300.0, ErrorMessage = "Label height must be between 10mm and 300mm")]
        public double LabelHeight
        {
            get => _labelHeight;
            set => _labelHeight = Math.Max(10.0, Math.Min(300.0, value));
        }

        /// <summary>
        /// Barcode width in millimeters
        /// </summary>
        [Range(5.0, 250.0, ErrorMessage = "Barcode width must be between 5mm and 250mm")]
        public double BarcodeWidth
        {
            get => _barcodeWidth;
            set => _barcodeWidth = Math.Max(5.0, Math.Min(250.0, value));
        }

        /// <summary>
        /// Barcode height in millimeters
        /// </summary>
        [Range(5.0, 100.0, ErrorMessage = "Barcode height must be between 5mm and 100mm")]
        public double BarcodeHeight
        {
            get => _barcodeHeight;
            set => _barcodeHeight = Math.Max(5.0, Math.Min(100.0, value));
        }

        /// <summary>
        /// Top margin for barcode positioning in millimeters
        /// </summary>
        public double TopMargin { get; set; } = 5.0;

        /// <summary>
        /// Bottom margin for text positioning in millimeters
        /// </summary>
        public double BottomMargin { get; set; } = 5.0;

        /// <summary>
        /// Left/Right margins for centering in millimeters
        /// </summary>
        public double HorizontalMargin { get; set; } = 5.0;

        /// <summary>
        /// Gap between barcode and description text in millimeters
        /// </summary>
        public double TextSpacing { get; set; } = 3.0;

        /// <summary>
        /// Font size for description text
        /// </summary>
        [Range(6, 24, ErrorMessage = "Font size must be between 6 and 24")]
        public int FontSize { get; set; } = 12;

        /// <summary>
        /// Validates if the barcode fits within the label dimensions
        /// </summary>
        /// <returns>True if barcode fits, false otherwise</returns>
        public bool IsValidLayout()
        {
            // Check if barcode fits within label width
            if (BarcodeWidth + (2 * HorizontalMargin) > LabelWidth)
                return false;

            // Check if barcode and text fit within label height
            double requiredHeight = TopMargin + BarcodeHeight + TextSpacing + FontSize + BottomMargin;
            if (requiredHeight > LabelHeight)
                return false;

            return true;
        }

        /// <summary>
        /// Gets validation error message if layout is invalid
        /// </summary>
        /// <returns>Error message or empty string if valid</returns>
        public string GetLayoutValidationError()
        {
            if (BarcodeWidth + (2 * HorizontalMargin) > LabelWidth)
                return "Barcode width exceeds label width";

            double requiredHeight = TopMargin + BarcodeHeight + TextSpacing + FontSize + BottomMargin;
            if (requiredHeight > LabelHeight)
                return "Content exceeds label height";

            return string.Empty;
        }

        /// <summary>
        /// Creates a copy of the current settings
        /// </summary>
        /// <returns>A new LabelSettings instance with the same values</returns>
        public LabelSettings Clone()
        {
            return new LabelSettings
            {
                LabelWidth = this.LabelWidth,
                LabelHeight = this.LabelHeight,
                BarcodeWidth = this.BarcodeWidth,
                BarcodeHeight = this.BarcodeHeight,
                TopMargin = this.TopMargin,
                BottomMargin = this.BottomMargin,
                HorizontalMargin = this.HorizontalMargin,
                TextSpacing = this.TextSpacing,
                FontSize = this.FontSize
            };
        }
    }
}