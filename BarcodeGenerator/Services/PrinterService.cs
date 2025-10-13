using System.Collections.ObjectModel;
using System.Drawing.Printing;
using BarcodeGenerator.Models;
using SystemPrinterSettings = System.Drawing.Printing.PrinterSettings;
using BarcodeGenerator.Helpers;

namespace BarcodeGenerator.Services
{
    /// <summary>
    /// Service for managing printer operations and Zebra printer communication
    /// </summary>
    public class PrinterService
    {
        private readonly BarcodeGeneratorService _barcodeGenerator;

        public PrinterService(BarcodeGeneratorService barcodeGenerator)
        {
            _barcodeGenerator = barcodeGenerator ?? throw new ArgumentNullException(nameof(barcodeGenerator));
        }

        /// <summary>
        /// Gets a list of all available printers on the system
        /// </summary>
        /// <returns>Collection of printer names</returns>
        public ObservableCollection<string> GetAvailablePrinters()
        {
            var printers = new ObservableCollection<string>();

            try
            {
                foreach (string printerName in SystemPrinterSettings.InstalledPrinters)
                {
                    printers.Add(printerName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting printers: {ex.Message}");
            }

            return printers;
        }

        /// <summary>
        /// Checks if the specified printer is a Zebra printer
        /// </summary>
        /// <param name="printerName">Name of the printer</param>
        /// <returns>True if it's a Zebra printer</returns>
        public bool IsZebraPrinter(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return false;

            return printerName.ToLowerInvariant().Contains("zebra");
        }

        /// <summary>
        /// Checks printer connection and accessibility
        /// </summary>
        /// <param name="printerName">Name of the printer</param>
        /// <returns>Connection status and result message</returns>
        public (bool IsConnected, string StatusMessage) CheckPrinterConnection(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return (false, "Printer name is empty");

            try
            {
                var (isAvailable, status) = RawPrinterHelper.GetPrinterInfo(printerName);
                return (isAvailable, status);
            }
            catch (Exception ex)
            {
                return (false, $"Connection check failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends ZPL command directly to the printer
        /// </summary>
        /// <param name="printerName">Name of the printer</param>
        /// <param name="zplCommand">ZPL command string</param>
        /// <returns>Success status and error message</returns>
        public (bool Success, string ErrorMessage) SendZplToPrinter(string printerName, string zplCommand)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return (false, "Printer name is required");

            if (string.IsNullOrWhiteSpace(zplCommand))
                return (false, "ZPL command is required");

            // Validate ZPL command
            var (isValid, validationError) = RawPrinterHelper.ValidateZplCommand(zplCommand);
            if (!isValid)
                return (false, $"Invalid ZPL command: {validationError}");

            try
            {
                return RawPrinterHelper.SendZplCommand(printerName, zplCommand);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to send ZPL command: {ex.Message}");
            }
        }

        /// <summary>
        /// Prints barcode labels with the specified data and settings
        /// </summary>
        /// <param name="barcodeData">Barcode data and description</param>
        /// <param name="labelSettings">Label dimensions and formatting</param>
        /// <param name="printerName">Target printer name</param>
        /// <param name="printerDpi">Printer DPI (default 203 for Zebra ZD220)</param>
        /// <returns>Print result with status and message</returns>
        public async Task<(bool Success, string Message)> PrintLabelsAsync(
            BarcodeData barcodeData, 
            LabelSettings labelSettings, 
            string printerName, 
            int printerDpi = 203)
        {
            // Validation
            if (barcodeData == null)
                return (false, "Barcode data is required");

            if (labelSettings == null)
                return (false, "Label settings are required");

            if (string.IsNullOrWhiteSpace(printerName))
                return (false, "Printer name is required");

            if (!barcodeData.IsValid())
                return (false, $"Invalid barcode data: {barcodeData.GetValidationError()}");

            if (!labelSettings.IsValidLayout())
                return (false, $"Invalid label layout: {labelSettings.GetLayoutValidationError()}");

            try
            {
                // Check printer connection
                var (isConnected, statusMessage) = CheckPrinterConnection(printerName);
                if (!isConnected)
                    return (false, $"Printer not available: {statusMessage}");

                // Generate ZPL command
                string zplCommand = ZplCommandGenerator.GenerateLabelZpl(barcodeData, labelSettings, printerDpi);

                // Send to printer
                var (success, errorMessage) = await Task.Run(() => SendZplToPrinter(printerName, zplCommand));

                if (success)
                {
                    string message = barcodeData.Copies == 1 
                        ? "Label printed successfully"
                        : $"{barcodeData.Copies} labels printed successfully";
                    return (true, message);
                }
                else
                {
                    return (false, $"Print failed: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Print operation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Prints multiple different labels in batch
        /// </summary>
        /// <param name="barcodeDataList">List of barcode data</param>
        /// <param name="labelSettings">Label settings</param>
        /// <param name="printerName">Target printer name</param>
        /// <param name="printerDpi">Printer DPI</param>
        /// <returns>Print result with status and message</returns>
        public async Task<(bool Success, string Message)> PrintBatchLabelsAsync(
            IEnumerable<BarcodeData> barcodeDataList,
            LabelSettings labelSettings,
            string printerName,
            int printerDpi = 203)
        {
            if (barcodeDataList == null || !barcodeDataList.Any())
                return (false, "No barcode data provided");

            try
            {
                // Validate all data
                int totalLabels = 0;
                foreach (var data in barcodeDataList)
                {
                    if (!data.IsValid())
                        return (false, $"Invalid barcode data: {data.GetValidationError()}");
                    totalLabels += data.Copies;
                }

                // Check printer connection
                var (isConnected, statusMessage) = CheckPrinterConnection(printerName);
                if (!isConnected)
                    return (false, $"Printer not available: {statusMessage}");

                // Generate batch ZPL command
                string batchZpl = ZplCommandGenerator.GenerateBatchLabelZpl(barcodeDataList, labelSettings, printerDpi);

                // Send to printer
                var (success, errorMessage) = await Task.Run(() => SendZplToPrinter(printerName, batchZpl));

                if (success)
                {
                    return (true, $"{totalLabels} labels printed successfully");
                }
                else
                {
                    return (false, $"Batch print failed: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Batch print operation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current status of the printer
        /// </summary>
        /// <param name="printerName">Name of the printer</param>
        /// <returns>Printer status information</returns>
        public PrinterStatus GetPrinterStatus(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return PrinterStatus.Unknown;

            try
            {
                var (isConnected, _) = CheckPrinterConnection(printerName);
                return isConnected ? PrinterStatus.Ready : PrinterStatus.Offline;
            }
            catch
            {
                return PrinterStatus.Error;
            }
        }

        /// <summary>
        /// Sends a test print to verify printer functionality
        /// </summary>
        /// <param name="printerName">Name of the printer</param>
        /// <returns>Test result with status and message</returns>
        public async Task<(bool Success, string Message)> TestPrintAsync(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return (false, "Printer name is required");

            try
            {
                // Check printer connection first
                var (isConnected, statusMessage) = CheckPrinterConnection(printerName);
                if (!isConnected)
                    return (false, $"Printer not available: {statusMessage}");

                // Generate test ZPL
                string testZpl = ZplCommandGenerator.GenerateTestLabelZpl();

                // Send test print
                var (success, errorMessage) = await Task.Run(() => SendZplToPrinter(printerName, testZpl));

                if (success)
                {
                    return (true, "Test label sent to printer successfully");
                }
                else
                {
                    return (false, $"Test print failed: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Test print operation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Configures printer settings (ZPL commands for Zebra printers)
        /// </summary>
        /// <param name="printerName">Name of the printer</param>
        /// <param name="printerSettings">Printer configuration</param>
        /// <returns>Configuration result</returns>
        public async Task<(bool Success, string Message)> ConfigurePrinterAsync(string printerName, Models.PrinterSettings printerSettings)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return (false, "Printer name is required");

            if (printerSettings == null)
                return (false, "Printer settings are required");

            if (!IsZebraPrinter(printerName))
                return (false, "Printer configuration is only supported for Zebra printers");

            try
            {
                // Generate configuration ZPL
                string configZpl = ZplCommandGenerator.GeneratePrinterConfigZpl(printerSettings);

                if (string.IsNullOrWhiteSpace(configZpl))
                    return (true, "No configuration changes needed");

                // Send configuration
                var (success, errorMessage) = await Task.Run(() => SendZplToPrinter(printerName, configZpl));

                if (success)
                {
                    return (true, "Printer configured successfully");
                }
                else
                {
                    return (false, $"Configuration failed: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Configuration operation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears the printer buffer
        /// </summary>
        /// <param name="printerName">Name of the printer</param>
        /// <returns>Clear operation result</returns>
        public async Task<(bool Success, string Message)> ClearPrinterBufferAsync(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return (false, "Printer name is required");

            if (!IsZebraPrinter(printerName))
                return (false, "Buffer clearing is only supported for Zebra printers");

            try
            {
                string clearZpl = ZplCommandGenerator.GenerateClearBufferZpl();
                
                var (success, errorMessage) = await Task.Run(() => SendZplToPrinter(printerName, clearZpl));

                if (success)
                {
                    return (true, "Printer buffer cleared successfully");
                }
                else
                {
                    return (false, $"Clear buffer failed: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Clear buffer operation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets printer capabilities and information
        /// </summary>
        /// <param name="printerName">Name of the printer</param>
        /// <returns>Printer information</returns>
        public PrinterInfo GetPrinterInfo(string printerName)
        {
            var info = new PrinterInfo
            {
                Name = printerName ?? string.Empty,
                IsZebraPrinter = IsZebraPrinter(printerName ?? string.Empty),
                Status = GetPrinterStatus(printerName ?? string.Empty)
            };

            try
            {
                var (isConnected, statusMessage) = CheckPrinterConnection(printerName ?? string.Empty);
                info.IsAvailable = isConnected;
                info.StatusMessage = statusMessage;
            }
            catch (Exception ex)
            {
                info.IsAvailable = false;
                info.StatusMessage = ex.Message;
            }

            return info;
        }

        /// <summary>
        /// Validates print settings before printing
        /// </summary>
        /// <param name="barcodeData">Barcode data</param>
        /// <param name="labelSettings">Label settings</param>
        /// <param name="printerName">Printer name</param>
        /// <returns>Validation result</returns>
        public (bool IsValid, string ErrorMessage) ValidatePrintSettings(
            BarcodeData barcodeData, 
            LabelSettings labelSettings, 
            string printerName)
        {
            if (barcodeData == null)
                return (false, "Barcode data is required");

            if (labelSettings == null)
                return (false, "Label settings are required");

            if (string.IsNullOrWhiteSpace(printerName))
                return (false, "Printer must be selected");

            if (!barcodeData.IsValid())
                return (false, barcodeData.GetValidationError());

            if (!labelSettings.IsValidLayout())
                return (false, labelSettings.GetLayoutValidationError());

            return (true, string.Empty);
        }
    }

    /// <summary>
    /// Information about a printer
    /// </summary>
    public class PrinterInfo
    {
        public string Name { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool IsZebraPrinter { get; set; }
        public PrinterStatus Status { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; } = DateTime.Now;
    }
}