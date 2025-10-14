using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BarcodeGenerator.Models
{
    /// <summary>
    /// Database entity + UI model representing a saved barcode record with all settings and metadata
    /// This class serves dual purpose: EF Core entity AND WPF binding model
    /// </summary>
    [Table("BarcodeRecords")]
    public partial class BarcodeRecord : ObservableValidator
    {
        /// <summary>
        /// Primary key, auto-increment
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        #region Core Barcode Data
        
        /// <summary>
        /// The barcode text/data (max 35 characters for Code 128)
        /// </summary>
        [Required]
        [MaxLength(35)]
        [ObservableProperty]
        private string _barcodeText = string.Empty;

        /// <summary>
        /// The actual barcode value that gets encoded (max 35 characters for Code 128)
        /// </summary>
        [Required]
        [MaxLength(35)]
        [ObservableProperty]
        private string _barcodeValue = string.Empty;

        /// <summary>
        /// Description text displayed below barcode (max 500 characters)
        /// </summary>
        [Required]
        [MaxLength(500)]
        [ObservableProperty]
        private string _description = string.Empty;

        /// <summary>
        /// Barcode type (default CODE128)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string BarcodeType { get; set; } = "CODE128";

        #endregion

        #region Date Tracking

        /// <summary>
        /// When the record was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// When this barcode was last printed (nullable)
        /// </summary>
        public DateTime? LastPrintedDate { get; set; }

        /// <summary>
        /// When this record was last modified (nullable)
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }

        /// <summary>
        /// When this barcode was last exported (nullable)
        /// </summary>
        public DateTime? LastExportedDate { get; set; }

        #endregion

        #region Counts & Status

        /// <summary>
        /// Total number of times this barcode has been printed
        /// </summary>
        [Required]
        [ObservableProperty]
        private int _totalPrintCount = 0;

        /// <summary>
        /// Default number of copies to print (remembers last quantity user printed)
        /// </summary>
        [Required]
        [ObservableProperty]
        private int _defaultLabelCount = 1;

        /// <summary>
        /// Soft delete flag - true if active, false if deleted
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether this barcode has been exported
        /// </summary>
        [Required]
        public bool IsExported { get; set; } = false;

        #endregion

        #region User Preferences - Remember Last Used Settings

        /// <summary>
        /// Last used label width in mm (nullable to allow default)
        /// </summary>
        [ObservableProperty]
        private double? _lastLabelWidth;

        /// <summary>
        /// Last used label height in mm (nullable to allow default)
        /// </summary>
        [ObservableProperty]
        private double? _lastLabelHeight;

        /// <summary>
        /// Last used barcode width in mm (nullable to allow default)
        /// </summary>
        [ObservableProperty]
        private double? _lastBarcodeWidth;

        /// <summary>
        /// Last used barcode height in mm (nullable to allow default)
        /// </summary>
        [ObservableProperty]
        private double? _lastBarcodeHeight;

        #endregion

        #region Additional Information

        /// <summary>
        /// Internal notes (max 1000 characters, nullable)
        /// </summary>
        [MaxLength(1000)]
        [ObservableProperty]
        private string? _notes;

        /// <summary>
        /// User comments (max 1000 characters, nullable)
        /// </summary>
        [MaxLength(1000)]
        [ObservableProperty]
        private string? _comment;

        /// <summary>
        /// Export filename if needed (max 200 characters, nullable)
        /// </summary>
        [MaxLength(200)]
        [ObservableProperty]
        private string? _filename;

        #endregion

        #region Navigation Properties

        /// <summary>
        /// One-to-many relationship with PrintHistory
        /// </summary>
        public virtual ICollection<PrintHistory> PrintHistories { get; set; } = new List<PrintHistory>();

        #endregion

        #region Helper Methods

        /// <summary>
        /// Updates the last modified date to current time
        /// </summary>
        public void MarkAsModified()
        {
            LastModifiedDate = DateTime.Now;
        }

        /// <summary>
        /// Marks as printed and updates counters
        /// </summary>
        /// <param name="quantity">Number of copies printed</param>
        public void MarkAsPrinted(int quantity)
        {
            LastPrintedDate = DateTime.Now;
            TotalPrintCount += quantity;
            MarkAsModified();
        }

        /// <summary>
        /// Updates the last used settings
        /// </summary>
        /// <param name="labelWidth">Label width in mm</param>
        /// <param name="labelHeight">Label height in mm</param>
        /// <param name="barcodeWidth">Barcode width in mm</param>
        /// <param name="barcodeHeight">Barcode height in mm</param>
        /// <param name="labelCount">Default label count</param>
        public void UpdateLastUsedSettings(double labelWidth, double labelHeight, 
            double barcodeWidth, double barcodeHeight, int labelCount)
        {
            LastLabelWidth = labelWidth;
            LastLabelHeight = labelHeight;
            LastBarcodeWidth = barcodeWidth;
            LastBarcodeHeight = barcodeHeight;
            DefaultLabelCount = labelCount;
            MarkAsModified();
        }

        /// <summary>
        /// Soft delete this record
        /// </summary>
        public void SoftDelete()
        {
            IsActive = false;
            MarkAsModified();
        }

        /// <summary>
        /// Restore this record from soft delete
        /// </summary>
        public void Restore()
        {
            IsActive = true;
            MarkAsModified();
        }

        /// <summary>
        /// Gets display text for UI binding
        /// </summary>
        public string DisplayText => $"{BarcodeText} - {Description}";

        /// <summary>
        /// Gets formatted created date for UI
        /// </summary>
        public string FormattedCreatedDate => CreatedDate.ToString("MM/dd/yyyy HH:mm");

        /// <summary>
        /// Gets formatted last printed date for UI
        /// </summary>
        public string FormattedLastPrintedDate => LastPrintedDate?.ToString("MM/dd/yyyy HH:mm") ?? "Never";

        #endregion
    }
}