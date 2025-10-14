using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BarcodeGenerator.Models
{
    /// <summary>
    /// Text alignment options for labels
    /// </summary>
    public enum LabelTextAlignment
    {
        Left,
        Center,
        Right
    }

    /// <summary>
    /// UI-only model representing label dimensions and barcode size settings (NOT a database entity)
    /// </summary>
    public partial class LabelSettings : ObservableObject
    {
        private double _labelWidth = 100.0; // Default 100mm
        private double _labelHeight = 60.0; // Default 60mm
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

        private int _descriptionFontSize = 18;
        private int _labelFontSize = 15;

        /// <summary>
        /// Font size for description text
        /// </summary>
        [Range(1, 32, ErrorMessage = "Description font size must be between 1 and 32")]
        public int DescriptionFontSize 
        { 
            get => _descriptionFontSize;
            set => _descriptionFontSize = Math.Max(1, Math.Min(32, value));
        }

        /// <summary>
        /// Font size for label text displayed below barcode
        /// </summary>
        [Range(1, 24, ErrorMessage = "Label font size must be between 1 and 24")]
        public int LabelFontSize 
        { 
            get => _labelFontSize;
            set => _labelFontSize = Math.Max(1, Math.Min(24, value));
        }

        /// <summary>
        /// Text alignment for label text (Center by default)
        /// </summary>
        public LabelTextAlignment LabelTextAlignment { get; set; } = LabelTextAlignment.Center;

        /// <summary>
        /// Text alignment for description text (Center by default)
        /// </summary>
        public LabelTextAlignment DescriptionTextAlignment { get; set; } = LabelTextAlignment.Center;

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
            // More realistic calculation considering text can be auto-resized
            // Reserve minimum space for text (at least 2 lines of 6pt font = ~8mm)
            double minTextSpaceMm = 8.0;
            double requiredHeight = TopMargin + BarcodeHeight + TextSpacing + minTextSpaceMm + BottomMargin;
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

            double minTextSpaceMm = 8.0;
            double requiredHeight = TopMargin + BarcodeHeight + TextSpacing + minTextSpaceMm + BottomMargin;
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
                DescriptionFontSize = this.DescriptionFontSize,
                LabelFontSize = this.LabelFontSize,
                LabelTextAlignment = this.LabelTextAlignment,
                DescriptionTextAlignment = this.DescriptionTextAlignment
            };
        }
    }
}