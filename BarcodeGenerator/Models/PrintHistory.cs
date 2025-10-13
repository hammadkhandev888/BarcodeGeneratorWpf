using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BarcodeGenerator.Models
{
    /// <summary>
    /// Database entity + UI model representing print history records
    /// This class serves dual purpose: EF Core entity AND WPF binding model
    /// </summary>
    [Table("PrintHistory")]
    public partial class PrintHistory : ObservableValidator
    {
        /// <summary>
        /// Primary key, auto-increment
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to BarcodeRecord
        /// </summary>
        [Required]
        public int BarcodeRecordId { get; set; }

        #region Print Details

        /// <summary>
        /// When the print job was executed
        /// </summary>
        [Required]
        public DateTime PrintedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Number of copies that were printed
        /// </summary>
        [Required]
        [ObservableProperty]
        private int _quantityPrinted;

        /// <summary>
        /// Name of the printer used
        /// </summary>
        [Required]
        [MaxLength(200)]
        [ObservableProperty]
        private string _printerName = string.Empty;

        #endregion

        #region Label Configuration at Time of Print

        /// <summary>
        /// Label width in mm at time of printing
        /// </summary>
        [Required]
        public double LabelWidth { get; set; }

        /// <summary>
        /// Label height in mm at time of printing
        /// </summary>
        [Required]
        public double LabelHeight { get; set; }

        /// <summary>
        /// Barcode width in mm at time of printing
        /// </summary>
        [Required]
        public double BarcodeWidth { get; set; }

        /// <summary>
        /// Barcode height in mm at time of printing
        /// </summary>
        [Required]
        public double BarcodeHeight { get; set; }

        #endregion

        #region Result Tracking

        /// <summary>
        /// Whether the print job succeeded
        /// </summary>
        [Required]
        [ObservableProperty]
        private bool _success = true;

        /// <summary>
        /// Error message if print failed (nullable)
        /// </summary>
        [MaxLength(1000)]
        [ObservableProperty]
        private string? _errorMessage;

        /// <summary>
        /// User who initiated the print (nullable)
        /// </summary>
        [MaxLength(100)]
        public string? UserName { get; set; } = Environment.UserName;

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Many-to-one relationship with BarcodeRecord
        /// </summary>
        [ForeignKey(nameof(BarcodeRecordId))]
        public virtual BarcodeRecord? BarcodeRecord { get; set; }

        #endregion

        #region Helper Properties for UI Binding

        /// <summary>
        /// Gets formatted printed date for UI
        /// </summary>
        public string FormattedPrintedDate => PrintedDate.ToString("MM/dd/yyyy HH:mm:ss");

        /// <summary>
        /// Gets formatted label size for UI
        /// </summary>
        public string FormattedLabelSize => $"{LabelWidth:F1} x {LabelHeight:F1} mm";

        /// <summary>
        /// Gets formatted barcode size for UI
        /// </summary>
        public string FormattedBarcodeSize => $"{BarcodeWidth:F1} x {BarcodeHeight:F1} mm";

        /// <summary>
        /// Gets status text for UI
        /// </summary>
        public string StatusText => Success ? "Success" : "Failed";

        /// <summary>
        /// Gets status color for UI
        /// </summary>
        public string StatusColor => Success ? "Green" : "Red";

        /// <summary>
        /// Gets display summary for UI
        /// </summary>
        public string DisplaySummary => $"{QuantityPrinted} copies on {PrinterName} - {StatusText}";

        #endregion

        #region Helper Methods

        /// <summary>
        /// Marks the print job as successful
        /// </summary>
        public void MarkAsSuccessful()
        {
            Success = true;
            ErrorMessage = null;
        }

        /// <summary>
        /// Marks the print job as failed with error message
        /// </summary>
        /// <param name="errorMessage">Error description</param>
        public void MarkAsFailed(string errorMessage)
        {
            Success = false;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Creates a print history record for a successful print
        /// </summary>
        /// <param name="barcodeRecordId">ID of the barcode record</param>
        /// <param name="quantityPrinted">Number of copies printed</param>
        /// <param name="printerName">Printer name</param>
        /// <param name="labelWidth">Label width in mm</param>
        /// <param name="labelHeight">Label height in mm</param>
        /// <param name="barcodeWidth">Barcode width in mm</param>
        /// <param name="barcodeHeight">Barcode height in mm</param>
        /// <returns>New PrintHistory instance</returns>
        public static PrintHistory CreateSuccessful(int barcodeRecordId, int quantityPrinted, 
            string printerName, double labelWidth, double labelHeight, 
            double barcodeWidth, double barcodeHeight)
        {
            return new PrintHistory
            {
                BarcodeRecordId = barcodeRecordId,
                QuantityPrinted = quantityPrinted,
                PrinterName = printerName,
                LabelWidth = labelWidth,
                LabelHeight = labelHeight,
                BarcodeWidth = barcodeWidth,
                BarcodeHeight = barcodeHeight,
                Success = true,
                PrintedDate = DateTime.Now
            };
        }

        /// <summary>
        /// Creates a print history record for a failed print
        /// </summary>
        /// <param name="barcodeRecordId">ID of the barcode record</param>
        /// <param name="quantityPrinted">Number of copies attempted</param>
        /// <param name="printerName">Printer name</param>
        /// <param name="labelWidth">Label width in mm</param>
        /// <param name="labelHeight">Label height in mm</param>
        /// <param name="barcodeWidth">Barcode width in mm</param>
        /// <param name="barcodeHeight">Barcode height in mm</param>
        /// <param name="errorMessage">Error message</param>
        /// <returns>New PrintHistory instance</returns>
        public static PrintHistory CreateFailed(int barcodeRecordId, int quantityPrinted, 
            string printerName, double labelWidth, double labelHeight, 
            double barcodeWidth, double barcodeHeight, string errorMessage)
        {
            return new PrintHistory
            {
                BarcodeRecordId = barcodeRecordId,
                QuantityPrinted = quantityPrinted,
                PrinterName = printerName,
                LabelWidth = labelWidth,
                LabelHeight = labelHeight,
                BarcodeWidth = barcodeWidth,
                BarcodeHeight = barcodeHeight,
                Success = false,
                ErrorMessage = errorMessage,
                PrintedDate = DateTime.Now
            };
        }

        #endregion
    }
}