using System.Collections.ObjectModel;

namespace BarcodeGenerator.Models
{
    /// <summary>
    /// Model representing printer settings and status
    /// </summary>
    public class PrinterSettings
    {
        /// <summary>
        /// Currently selected printer name
        /// </summary>
        public string SelectedPrinter { get; set; } = string.Empty;

        /// <summary>
        /// List of available printers on the system
        /// </summary>
        public ObservableCollection<string> AvailablePrinters { get; set; } = new();

        /// <summary>
        /// Indicates if the selected printer is a Zebra printer
        /// </summary>
        public bool IsZebraPrinter => !string.IsNullOrEmpty(SelectedPrinter) && 
                                     SelectedPrinter.ToLowerInvariant().Contains("zebra");

        /// <summary>
        /// Printer DPI setting (dots per inch)
        /// Default is 203 DPI for Zebra ZD220
        /// </summary>
        public int PrinterDpi { get; set; } = 203;

        /// <summary>
        /// Print speed setting (inches per second)
        /// </summary>
        public int PrintSpeed { get; set; } = 4;

        /// <summary>
        /// Print density/darkness setting (0-30)
        /// </summary>
        public int PrintDensity { get; set; } = 8;

        /// <summary>
        /// Indicates if printer status should be checked before printing
        /// </summary>
        public bool CheckPrinterStatus { get; set; } = true;

        /// <summary>
        /// Timeout for printer operations in milliseconds
        /// </summary>
        public int PrinterTimeout { get; set; } = 5000;

        /// <summary>
        /// Last known printer status
        /// </summary>
        public PrinterStatus LastStatus { get; set; } = PrinterStatus.Unknown;

        /// <summary>
        /// Last error message from printer operations
        /// </summary>
        public string LastError { get; set; } = string.Empty;

        /// <summary>
        /// Validates if printer settings are valid for printing
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValidForPrinting()
        {
            return !string.IsNullOrWhiteSpace(SelectedPrinter) &&
                   PrinterDpi > 0 &&
                   PrintSpeed > 0 &&
                   PrintDensity >= 0 && PrintDensity <= 30;
        }

        /// <summary>
        /// Gets validation error message for printer settings
        /// </summary>
        /// <returns>Error message or empty string if valid</returns>
        public string GetValidationError()
        {
            if (string.IsNullOrWhiteSpace(SelectedPrinter))
                return "No printer selected";

            if (PrinterDpi <= 0)
                return "Invalid printer DPI setting";

            if (PrintSpeed <= 0)
                return "Invalid print speed setting";

            if (PrintDensity < 0 || PrintDensity > 30)
                return "Print density must be between 0 and 30";

            return string.Empty;
        }
    }

    /// <summary>
    /// Enumeration of possible printer status values
    /// </summary>
    public enum PrinterStatus
    {
        Unknown,
        Ready,
        Offline,
        OutOfPaper,
        PaperJam,
        CoverOpen,
        Error,
        Busy
    }

    /// <summary>
    /// Model for application-wide settings
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Last used label settings
        /// </summary>
        public LabelSettings LabelSettings { get; set; } = new();

        /// <summary>
        /// Printer configuration
        /// </summary>
        public PrinterSettings PrinterSettings { get; set; } = new();

        /// <summary>
        /// Last used barcode data (for convenience)
        /// </summary>
        public BarcodeData LastBarcodeData { get; set; } = new();

        /// <summary>
        /// Window position and size settings
        /// </summary>
        public WindowSettings WindowSettings { get; set; } = new();

        /// <summary>
        /// Application preferences
        /// </summary>
        public bool AutoSaveSettings { get; set; } = true;
        public bool ShowPreview { get; set; } = true;
        public bool ValidateOnInput { get; set; } = true;
        public int PreviewUpdateDelay { get; set; } = 300; // milliseconds

        /// <summary>
        /// Creates default application settings
        /// </summary>
        /// <returns>New AppSettings with default values</returns>
        public static AppSettings CreateDefault()
        {
            return new AppSettings
            {
                LabelSettings = new LabelSettings(),
                PrinterSettings = new PrinterSettings(),
                LastBarcodeData = new BarcodeData(),
                WindowSettings = new WindowSettings()
            };
        }
    }

    /// <summary>
    /// Model for window position and size settings
    /// </summary>
    public class WindowSettings
    {
        public double Left { get; set; } = 100;
        public double Top { get; set; } = 100;
        public double Width { get; set; } = 800;
        public double Height { get; set; } = 600;
        public bool IsMaximized { get; set; } = false;
    }
}