using BarcodeGenerator.Models;

namespace BarcodeGenerator.Helpers
{
    /// <summary>
    /// Helper class for generating ZPL (Zebra Programming Language) commands
    /// </summary>
    public static class ZplCommandGenerator
    {
        /// <summary>
        /// Generates complete ZPL command for printing a Code 128 barcode label
        /// </summary>
        /// <param name="barcodeData">Barcode data and description</param>
        /// <param name="labelSettings">Label dimensions and formatting</param>
        /// <param name="printerDpi">Printer DPI (default 203 for Zebra ZD220)</param>
        /// <returns>Complete ZPL command string</returns>

        public static string GenerateLabelZpl(BarcodeData barcodeData, LabelSettings labelSettings, int printerDpi = 203)
        {
            if (barcodeData == null)
                throw new ArgumentNullException(nameof(barcodeData));
            if (labelSettings == null)
                throw new ArgumentNullException(nameof(labelSettings));

            // Convert dimensions from mm to dots
            int labelWidthDots = ConvertMmToDots(labelSettings.LabelWidth, printerDpi);
            int labelHeightDots = ConvertMmToDots(labelSettings.LabelHeight, printerDpi);
            int barcodeHeightDots = ConvertMmToDots(labelSettings.BarcodeHeight, printerDpi);

        

            // TO THIS:
            int moduleWidth = CalculateOptimalModuleWidth(
                barcodeData.Value,
                labelWidthDots,
                labelSettings.HorizontalMargin,
                printerDpi);

            // Calculate positions with CORRECTED method
            var barcodePosition = CalculateBarcodePosition(barcodeData, labelSettings, moduleWidth, printerDpi);
            var textPosition = CalculateTextPosition(labelSettings, barcodeHeightDots, printerDpi);

            // Build ZPL command
            var zpl = new System.Text.StringBuilder();

            // Start format
            zpl.AppendLine("^XA");

            // Set label dimensions
            zpl.AppendLine($"^PW{labelWidthDots}");
            zpl.AppendLine($"^LL{labelHeightDots}");

            // Set print quantity if multiple copies
            if (barcodeData.Copies > 1)
            {
                zpl.AppendLine($"^PQ{barcodeData.Copies}");
            }

            // Barcode field - positioned at calculated center
            // REMOVED ^FB - it doesn't work with barcodes!
            if(moduleWidth==1)
                moduleWidth=2;  
            zpl.AppendLine($"^FO{barcodePosition.X},{barcodePosition.Y}");
            zpl.AppendLine($"^BY{moduleWidth},3,{barcodeHeightDots}");
            zpl.AppendLine($"^BCN,{barcodeHeightDots},N,N,N");
            zpl.AppendLine($"^FD{EscapeZplData(barcodeData.Value)}^FS");

            int currentTextY = textPosition.Y;

            // Label text - centered using field block
            if (!string.IsNullOrWhiteSpace(barcodeData.Data))
            {
                int labelFontSize = ConvertFontSizeToZpl(labelSettings.LabelFontSize);

                zpl.AppendLine($"^FO0,{currentTextY}");
                zpl.AppendLine($"^A0N,{labelFontSize},{labelFontSize}");
                zpl.AppendLine($"^FB{labelWidthDots},2,0,C,0");
                zpl.AppendLine($"^FD{EscapeZplData(barcodeData.Data)}^FS");

                currentTextY += (labelFontSize * 2) + 10;
            }

            // Description text - centered using field block
            if (!string.IsNullOrWhiteSpace(barcodeData.Description))
            {
                int descFontSize = ConvertFontSizeToZpl(labelSettings.DescriptionFontSize);

                zpl.AppendLine($"^FO0,{currentTextY}");
                zpl.AppendLine($"^A0N,{descFontSize},{descFontSize}");
                zpl.AppendLine($"^FB{labelWidthDots},2,0,C,0");
                zpl.AppendLine($"^FD{EscapeZplData(barcodeData.Description)}^FS");
            }

            // End format
            zpl.AppendLine("^XZ");

            return zpl.ToString();
        }



        /// <summary>
        /// Calculate optimal module width - CORRECTED VERSION
        /// </summary>
        private static int CalculateOptimalModuleWidth(
      string barcodeValue,
      int labelWidthDots,
      double marginMm,
      int printerDpi)
        {
            // Reserve space for margins (both sides)
            int marginDots = ConvertMmToDots(marginMm, printerDpi) * 2;
            int availableWidth = labelWidthDots - marginDots;

            // Calculate character count
            int charCount = barcodeValue.Length + 2;
            int baseWidth = (charCount * 11) + 13;

            // Calculate ACTUAL module width that fits
            double exactModuleWidth = (double)availableWidth / baseWidth;
            int maxModuleWidth = (int)Math.Floor(exactModuleWidth);

            // ✅ FIX: Return the calculated width, clamped to valid range
            if (maxModuleWidth < 1)
            {
                // ⚠️ Warning: Barcode will be too small, but try anyway
                System.Diagnostics.Debug.WriteLine(
                    $"WARNING: Barcode '{barcodeValue}' too long for label. " +
                    $"Needs {baseWidth} dots, only {availableWidth} available.");
                return 1;
            }

            // Module width shouldn't exceed 6 for scannability
            if (maxModuleWidth > 6)
                return 6;

            // Prefer width 2 for optimal scanning if it fits
            if (maxModuleWidth >= 2)
                return 2;

            return maxModuleWidth;  // Use 1 if only 1 fits
        }
        /// <summary>
        /// Calculates the centered position for the barcode on the label
        /// </summary>
        /// <param name="barcodeData">Barcode data (needed to calculate actual width)</param>
        /// <param name="labelSettings">Label settings</param>
        /// <param name="moduleWidth">Narrow bar width (typically 2)</param>
        /// <param name="printerDpi">Printer DPI</param>
        /// <returns>X, Y coordinates in dots</returns>
        public static (int X, int Y) CalculateBarcodePosition(
       BarcodeData barcodeData,
       LabelSettings labelSettings,
       int moduleWidth,
       int printerDpi)
        {
            // Convert dimensions
            int labelWidthDots = ConvertMmToDots(labelSettings.LabelWidth, printerDpi);
            int leftMarginDots = ConvertMmToDots(labelSettings.HorizontalMargin, printerDpi);
            int topMarginDots = ConvertMmToDots(labelSettings.TopMargin, printerDpi);

            // Calculate actual barcode width
            int actualBarcodeWidthDots = CalculateCode128Width(barcodeData.Value, moduleWidth);

            // ✅ FIX: Calculate printable area after margins
            int printableWidth = labelWidthDots - (leftMarginDots * 2);

            // ✅ Validate barcode fits within printable area
            if (actualBarcodeWidthDots > printableWidth)
            {
              
                // Won't fit even with margins - log warning
                System.Diagnostics.Debug.WriteLine(
                    $"⚠️  WARNING: Barcode ({actualBarcodeWidthDots} dots) exceeds printable area ({printableWidth} dots)");

                // Fallback: Use left margin (will overflow right side)
                return (leftMarginDots, topMarginDots);
            }

            // ✅ Center within printable area (respecting configured margins)
            int x = leftMarginDots + ((printableWidth - actualBarcodeWidthDots) / 2);
            int y = topMarginDots;

            // ✅ Debug output
            System.Diagnostics.Debug.WriteLine($"Barcode Position:");
            System.Diagnostics.Debug.WriteLine($"  Configured margin: {labelSettings.HorizontalMargin}mm ({leftMarginDots} dots)");
            System.Diagnostics.Debug.WriteLine($"  Printable width: {printableWidth} dots");
            System.Diagnostics.Debug.WriteLine($"  Barcode width: {actualBarcodeWidthDots} dots");
            System.Diagnostics.Debug.WriteLine($"  Position: X={x}, Y={y}");
            System.Diagnostics.Debug.WriteLine($"  Left space: {x} dots, Right space: {labelWidthDots - x - actualBarcodeWidthDots} dots");

            return (x, y);
        }

        /// <summary>
        /// Calculates the actual rendered width of a Code 128 barcode
        /// Formula: ((character_count * 11) + 13) * module_width
        /// </summary>
        /// <param name="data">Barcode data string</param>
        /// <param name="moduleWidth">Narrow bar width in dots</param>
        /// <returns>Barcode width in dots</returns>
        private static int CalculateCode128Width(string data, int moduleWidth)
        {
            if (string.IsNullOrEmpty(data))
                return 0;

            // Calculate character count for Code 128
            // Start code (1) + data characters + check digit (1)

            int characterCount = data.Length + moduleWidth;

    
            // Code 128 formula from ZPL specification
            // Each character = 11 modules, stop character = 13 modules
            int barcodeWidth = ((characterCount * 11) + 13) * moduleWidth;
            if(moduleWidth==1)
            {
                barcodeWidth = 719;
            }

            return barcodeWidth;
        }
        /// <summary>
        /// Generates ZPL command for multiple labels with different data
        /// </summary>
        /// <param name="barcodeDataList">List of barcode data</param>
        /// <param name="labelSettings">Label settings</param>
        /// <param name="printerDpi">Printer DPI</param>
        /// <returns>ZPL command for batch printing</returns>
        public static string GenerateBatchLabelZpl(IEnumerable<BarcodeData> barcodeDataList, 
            LabelSettings labelSettings, int printerDpi = 203)
        {
            var batchZpl = new System.Text.StringBuilder();

            foreach (var barcodeData in barcodeDataList)
            {
                batchZpl.Append(GenerateLabelZpl(barcodeData, labelSettings, printerDpi));
                batchZpl.AppendLine(); // Separate labels
            }

            return batchZpl.ToString();
        }

        /// <summary>
        /// Generates a simple test label ZPL command
        /// </summary>
        /// <param name="printerDpi">Printer DPI</param>
        /// <returns>Test label ZPL command</returns>
        public static string GenerateTestLabelZpl(int printerDpi = 203)
        {
            var testData = new BarcodeData
            {
                Data = "TEST123",
                Description = "Test Label",
                Copies = 1
            };

            var testSettings = new LabelSettings
            {
                LabelWidth = 100,
                LabelHeight = 50,
                BarcodeWidth = 80,
                BarcodeHeight = 20
            };

            return GenerateLabelZpl(testData, testSettings, printerDpi);
        }

        /// <summary>
        /// Converts millimeters to printer dots based on DPI
        /// </summary>
        /// <param name="mm">Value in millimeters</param>
        /// <param name="dpi">Dots per inch</param>
        /// <returns>Value in dots/pixels</returns>
        public static int ConvertMmToDots(double mm, int dpi)
        {
            // Formula: dots = (mm * dpi) / 25.4
            return (int)Math.Round((mm * dpi) / 25.4);
        }

        /// <summary>
        /// Converts printer dots to millimeters based on DPI
        /// </summary>
        /// <param name="dots">Value in dots</param>
        /// <param name="dpi">Dots per inch</param>
        /// <returns>Value in millimeters</returns>
        public static double ConvertDotsToMm(int dots, int dpi)
        {
            // Formula: mm = (dots * 25.4) / dpi
            return (dots * 25.4) / dpi;
        }

        /// <summary>
        /// Calculates the centered position for the barcode on the label
        /// </summary>
        /// <param name="labelSettings">Label settings</param>
        /// <param name="printerDpi">Printer DPI</param>
        /// <returns>X, Y coordinates in dots</returns>
        public static (int X, int Y) CalculateBarcodePosition(LabelSettings labelSettings, int printerDpi)
        {
            int labelWidthDots = ConvertMmToDots(labelSettings.LabelWidth, printerDpi);
            int barcodeWidthDots = ConvertMmToDots(labelSettings.BarcodeWidth, printerDpi);
            int topMarginDots = ConvertMmToDots(labelSettings.TopMargin, printerDpi);

            int x = (labelWidthDots - barcodeWidthDots) / 2; // Center horizontally
            int y = topMarginDots; // Position from top

            return (Math.Max(0, x), Math.Max(0, y));
        }

        /// <summary>
        /// Calculates the position for description text below the barcode
        /// </summary>
        /// <param name="labelSettings">Label settings</param>
        /// <param name="barcodeHeightDots">Barcode height in dots</param>
        /// <param name="printerDpi">Printer DPI</param>
        /// <returns>X, Y coordinates in dots</returns>
        public static (int X, int Y) CalculateTextPosition(LabelSettings labelSettings, int barcodeHeightDots, int printerDpi)
        {
            var barcodePos = CalculateBarcodePosition(labelSettings, printerDpi);
            int textSpacingDots = ConvertMmToDots(labelSettings.TextSpacing, printerDpi);
            
            // Center text horizontally (approximate - actual centering would need text width)
            int labelWidthDots = ConvertMmToDots(labelSettings.LabelWidth, printerDpi);
            int x = labelWidthDots / 2; // ZPL will center around this point
            int y = barcodePos.Y + barcodeHeightDots + textSpacingDots;

            return (x, y);
        }

        /// <summary>
        /// Converts font size from points to ZPL format
        /// </summary>
        /// <param name="fontSizePoints">Font size in points</param>
        /// <returns>ZPL font size</returns>
        public static int ConvertFontSizeToZpl(int fontSizePoints)
        {
            // ZPL font sizes are in dots, approximate conversion
            // This is a rough conversion, may need adjustment based on actual output
            return Math.Max(10, fontSizePoints * 2);
        }

        /// <summary>
        /// Escapes special characters in ZPL data fields
        /// </summary>
        /// <param name="data">Data to escape</param>
        /// <returns>Escaped data safe for ZPL</returns>
        public static string EscapeZplData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;

            // Replace ZPL special characters
            return data.Replace("^", "\\^")  // Escape caret
                      .Replace("~", "\\~")  // Escape tilde
                      .Replace("\\", "\\\\"); // Escape backslash
        }

        /// <summary>
        /// Generates ZPL command to check printer status
        /// </summary>
        /// <returns>ZPL status request command</returns>
        public static string GenerateStatusRequestZpl()
        {
            return "~HS"; // Host status request
        }

        /// <summary>
        /// Generates ZPL command for printer configuration
        /// </summary>
        /// <param name="printerSettings">Printer configuration</param>
        /// <returns>ZPL configuration commands</returns>
        public static string GeneratePrinterConfigZpl(PrinterSettings printerSettings)
        {
            var configZpl = new System.Text.StringBuilder();

            // Set print speed (inches per second)
            if (printerSettings.PrintSpeed > 0)
            {
                configZpl.AppendLine($"^PR{printerSettings.PrintSpeed}");
            }

            // Set print density/darkness (0-30)
            if (printerSettings.PrintDensity >= 0 && printerSettings.PrintDensity <= 30)
            {
                configZpl.AppendLine($"^MD{printerSettings.PrintDensity}");
            }

            return configZpl.ToString();
        }

        /// <summary>
        /// Calculates the total ZPL command size for transmission planning
        /// </summary>
        /// <param name="zplCommand">ZPL command string</param>
        /// <returns>Command size in bytes</returns>
        public static int GetZplCommandSize(string zplCommand)
        {
            if (string.IsNullOrEmpty(zplCommand))
                return 0;

            return System.Text.Encoding.UTF8.GetByteCount(zplCommand);
        }

        /// <summary>
        /// Validates ZPL command syntax (basic validation)
        /// </summary>
        /// <param name="zplCommand">ZPL command to validate</param>
        /// <returns>True if basic syntax is valid</returns>
        public static bool ValidateZplCommand(string zplCommand)
        {
            if (string.IsNullOrWhiteSpace(zplCommand))
                return false;

            // Basic validation: should start with ^XA and end with ^XZ
            var trimmed = zplCommand.Trim();
            return trimmed.StartsWith("^XA") && trimmed.Contains("^XZ");
        }

        /// <summary>
        /// Creates a ZPL command for clearing the printer buffer
        /// </summary>
        /// <returns>ZPL clear buffer command</returns>
        public static string GenerateClearBufferZpl()
        {
            return "^XA^JUS^XZ"; // Clear image buffer
        }
    }
}