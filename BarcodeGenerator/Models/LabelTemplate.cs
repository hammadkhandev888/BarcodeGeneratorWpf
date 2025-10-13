using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BarcodeGenerator.Models
{
    /// <summary>
    /// Database entity + UI model representing label size templates for quick selection
    /// This class serves dual purpose: EF Core entity AND WPF binding model
    /// </summary>
    [Table("LabelTemplates")]
    public partial class LabelTemplate : ObservableValidator
    {
        /// <summary>
        /// Primary key, auto-increment
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Template name for display (e.g., "Standard 100x50")
        /// </summary>
        [Required]
        [MaxLength(100)]
        [ObservableProperty]
        private string _templateName = string.Empty;

        /// <summary>
        /// Label width in millimeters
        /// </summary>
        [Required]
        [ObservableProperty]
        private double _labelWidth;

        /// <summary>
        /// Label height in millimeters
        /// </summary>
        [Required]
        [ObservableProperty]
        private double _labelHeight;

        /// <summary>
        /// Barcode width in millimeters
        /// </summary>
        [Required]
        [ObservableProperty]
        private double _barcodeWidth;

        /// <summary>
        /// Barcode height in millimeters
        /// </summary>
        [Required]
        [ObservableProperty]
        private double _barcodeHeight;

        /// <summary>
        /// Whether this is the default template
        /// </summary>
        [Required]
        [ObservableProperty]
        private bool _isDefault = false;

        /// <summary>
        /// When this template was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        #region Helper Properties for UI Binding

        /// <summary>
        /// Gets formatted dimensions for UI display
        /// </summary>
        public string FormattedDimensions => $"{LabelWidth:F1} x {LabelHeight:F1} mm";

        /// <summary>
        /// Gets formatted barcode size for UI display
        /// </summary>
        public string FormattedBarcodeSize => $"Barcode: {BarcodeWidth:F1} x {BarcodeHeight:F1} mm";

        /// <summary>
        /// Gets display text combining name and dimensions
        /// </summary>
        public string DisplayText => $"{TemplateName} ({FormattedDimensions})";

        /// <summary>
        /// Gets default indicator text for UI
        /// </summary>
        public string DefaultIndicator => IsDefault ? " (Default)" : string.Empty;

        /// <summary>
        /// Gets full display text with default indicator
        /// </summary>
        public string FullDisplayText => $"{DisplayText}{DefaultIndicator}";

        #endregion

        #region Helper Methods

        /// <summary>
        /// Sets this template as the default (should be used with database update)
        /// </summary>
        public void SetAsDefault()
        {
            IsDefault = true;
        }

        /// <summary>
        /// Removes default status from this template
        /// </summary>
        public void RemoveDefault()
        {
            IsDefault = false;
        }

        /// <summary>
        /// Updates template dimensions
        /// </summary>
        /// <param name="labelWidth">Label width in mm</param>
        /// <param name="labelHeight">Label height in mm</param>
        /// <param name="barcodeWidth">Barcode width in mm</param>
        /// <param name="barcodeHeight">Barcode height in mm</param>
        public void UpdateDimensions(double labelWidth, double labelHeight, 
            double barcodeWidth, double barcodeHeight)
        {
            LabelWidth = labelWidth;
            LabelHeight = labelHeight;
            BarcodeWidth = barcodeWidth;
            BarcodeHeight = barcodeHeight;
        }

        /// <summary>
        /// Creates a new template with common dimensions
        /// </summary>
        /// <param name="name">Template name</param>
        /// <param name="labelWidth">Label width in mm</param>
        /// <param name="labelHeight">Label height in mm</param>
        /// <param name="barcodeWidth">Barcode width in mm</param>
        /// <param name="barcodeHeight">Barcode height in mm</param>
        /// <param name="isDefault">Whether this should be the default</param>
        /// <returns>New LabelTemplate instance</returns>
        public static LabelTemplate Create(string name, double labelWidth, double labelHeight,
            double barcodeWidth, double barcodeHeight, bool isDefault = false)
        {
            return new LabelTemplate
            {
                TemplateName = name,
                LabelWidth = labelWidth,
                LabelHeight = labelHeight,
                BarcodeWidth = barcodeWidth,
                BarcodeHeight = barcodeHeight,
                IsDefault = isDefault,
                CreatedDate = DateTime.Now
            };
        }

        /// <summary>
        /// Creates the standard 100x50mm template
        /// </summary>
        /// <returns>Standard template</returns>
        public static LabelTemplate CreateStandard()
        {
            return Create("Standard 100x50mm", 100.0, 50.0, 80.0, 20.0, true);
        }

        /// <summary>
        /// Creates a small 75x25mm template
        /// </summary>
        /// <returns>Small template</returns>
        public static LabelTemplate CreateSmall()
        {
            return Create("Small 75x25mm", 75.0, 25.0, 60.0, 15.0, false);
        }

        /// <summary>
        /// Creates a large 150x75mm template
        /// </summary>
        /// <returns>Large template</returns>
        public static LabelTemplate CreateLarge()
        {
            return Create("Large 150x75mm", 150.0, 75.0, 120.0, 30.0, false);
        }

        /// <summary>
        /// Validates template dimensions
        /// </summary>
        /// <returns>True if dimensions are valid</returns>
        public bool IsValid()
        {
            return LabelWidth > 0 && LabelHeight > 0 && 
                   BarcodeWidth > 0 && BarcodeHeight > 0 &&
                   BarcodeWidth <= LabelWidth && BarcodeHeight <= LabelHeight &&
                   !string.IsNullOrWhiteSpace(TemplateName);
        }

        /// <summary>
        /// Gets validation error message
        /// </summary>
        /// <returns>Error message or empty string if valid</returns>
        public string GetValidationError()
        {
            if (string.IsNullOrWhiteSpace(TemplateName))
                return "Template name is required";

            if (LabelWidth <= 0 || LabelHeight <= 0)
                return "Label dimensions must be positive";

            if (BarcodeWidth <= 0 || BarcodeHeight <= 0)
                return "Barcode dimensions must be positive";

            if (BarcodeWidth > LabelWidth)
                return "Barcode width cannot exceed label width";

            if (BarcodeHeight > LabelHeight)
                return "Barcode height cannot exceed label height";

            return string.Empty;
        }

        #endregion
    }
}