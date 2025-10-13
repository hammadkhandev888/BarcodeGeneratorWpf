namespace BarcodeGenerator.Helpers
{
    /// <summary>
    /// Helper class for input validation
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates barcode text for Code 128 compatibility
        /// </summary>
        /// <param name="text">Text to validate</param>
        /// <returns>Validation result with error message</returns>
        public static (bool IsValid, string ErrorMessage) ValidateBarcodeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return (false, "Barcode text is required");

            if (text.Length > 35)
                return (false, "Barcode text cannot exceed 35 characters");

            // Check for Code 128 valid characters (ASCII 0-127)
            foreach (char c in text)
            {
                if (c > 127)
                    return (false, "Barcode text contains invalid characters. Only ASCII characters (0-127) are allowed.");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Validates label dimensions
        /// </summary>
        /// <param name="width">Width in millimeters</param>
        /// <param name="height">Height in millimeters</param>
        /// <returns>Validation result with error message</returns>
        public static (bool IsValid, string ErrorMessage) ValidateLabelSize(double width, double height)
        {
            const double minSize = 10.0;
            const double maxSize = 300.0;

            if (width < minSize || width > maxSize)
                return (false, $"Label width must be between {minSize}mm and {maxSize}mm");

            if (height < minSize || height > maxSize)
                return (false, $"Label height must be between {minSize}mm and {maxSize}mm");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validates barcode dimensions
        /// </summary>
        /// <param name="width">Width in millimeters</param>
        /// <param name="height">Height in millimeters</param>
        /// <returns>Validation result with error message</returns>
        public static (bool IsValid, string ErrorMessage) ValidateBarcodeSize(double width, double height)
        {
            const double minWidth = 5.0;
            const double maxWidth = 250.0;
            const double minHeight = 5.0;
            const double maxHeight = 100.0;

            if (width < minWidth || width > maxWidth)
                return (false, $"Barcode width must be between {minWidth}mm and {maxWidth}mm");

            if (height < minHeight || height > maxHeight)
                return (false, $"Barcode height must be between {minHeight}mm and {maxHeight}mm");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validates numeric input from text box
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="min">Minimum allowed value</param>
        /// <param name="max">Maximum allowed value</param>
        /// <returns>True if valid number within range</returns>
        public static bool ValidateNumericInput(string input, double min, double max)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (!double.TryParse(input, out double value))
                return false;

            return value >= min && value <= max;
        }

        /// <summary>
        /// Validates and parses numeric input
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="min">Minimum allowed value</param>
        /// <param name="max">Maximum allowed value</param>
        /// <returns>Parsed value and validation result</returns>
        public static (bool IsValid, double Value, string ErrorMessage) ParseAndValidateNumeric(string input, double min, double max)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (false, 0, "Value is required");

            if (!double.TryParse(input, out double value))
                return (false, 0, "Invalid numeric format");

            if (value < min)
                return (false, value, $"Value must be at least {min}");

            if (value > max)
                return (false, value, $"Value cannot exceed {max}");

            return (true, value, string.Empty);
        }

        /// <summary>
        /// Validates integer input from text box
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="min">Minimum allowed value</param>
        /// <param name="max">Maximum allowed value</param>
        /// <returns>True if valid integer within range</returns>
        public static bool ValidateIntegerInput(string input, int min, int max)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (!int.TryParse(input, out int value))
                return false;

            return value >= min && value <= max;
        }

        /// <summary>
        /// Validates and parses integer input
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="min">Minimum allowed value</param>
        /// <param name="max">Maximum allowed value</param>
        /// <returns>Parsed value and validation result</returns>
        public static (bool IsValid, int Value, string ErrorMessage) ParseAndValidateInteger(string input, int min, int max)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (false, 0, "Value is required");

            if (!int.TryParse(input, out int value))
                return (false, 0, "Invalid integer format");

            if (value < min)
                return (false, value, $"Value must be at least {min}");

            if (value > max)
                return (false, value, $"Value cannot exceed {max}");

            return (true, value, string.Empty);
        }

        /// <summary>
        /// Validates print copies input
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>Validation result</returns>
        public static (bool IsValid, int Copies, string ErrorMessage) ValidateCopies(string input)
        {
            return ParseAndValidateInteger(input, 1, 999);
        }

        /// <summary>
        /// Validates description text length
        /// </summary>
        /// <param name="text">Description text</param>
        /// <param name="maxLength">Maximum allowed length</param>
        /// <returns>Validation result</returns>
        public static (bool IsValid, string ErrorMessage) ValidateDescription(string text, int maxLength = 100)
        {
            if (text == null)
                return (true, string.Empty); // Description is optional

            if (text.Length > maxLength)
                return (false, $"Description cannot exceed {maxLength} characters");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validates printer name
        /// </summary>
        /// <param name="printerName">Printer name</param>
        /// <returns>Validation result</returns>
        public static (bool IsValid, string ErrorMessage) ValidatePrinterName(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return (false, "Printer name is required");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validates font size
        /// </summary>
        /// <param name="fontSize">Font size</param>
        /// <returns>Validation result</returns>
        public static (bool IsValid, string ErrorMessage) ValidateFontSize(int fontSize)
        {
            const int minFontSize = 6;
            const int maxFontSize = 72;

            if (fontSize < minFontSize || fontSize > maxFontSize)
                return (false, $"Font size must be between {minFontSize} and {maxFontSize}");

            return (true, string.Empty);
        }

        /// <summary>
        /// Validates margin values
        /// </summary>
        /// <param name="margin">Margin in millimeters</param>
        /// <returns>Validation result</returns>
        public static (bool IsValid, string ErrorMessage) ValidateMargin(double margin)
        {
            const double minMargin = 0.0;
            const double maxMargin = 50.0;

            if (margin < minMargin || margin > maxMargin)
                return (false, $"Margin must be between {minMargin}mm and {maxMargin}mm");

            return (true, string.Empty);
        }

        /// <summary>
        /// Checks if barcode fits within label dimensions
        /// </summary>
        /// <param name="labelWidth">Label width in mm</param>
        /// <param name="labelHeight">Label height in mm</param>
        /// <param name="barcodeWidth">Barcode width in mm</param>
        /// <param name="barcodeHeight">Barcode height in mm</param>
        /// <param name="margins">Total margins (top + bottom, left + right)</param>
        /// <param name="textHeight">Estimated text height in mm</param>
        /// <returns>Validation result</returns>
        public static (bool IsValid, string ErrorMessage) ValidateLayout(
            double labelWidth, double labelHeight,
            double barcodeWidth, double barcodeHeight,
            (double Horizontal, double Vertical) margins,
            double textHeight = 5.0)
        {
            // Check horizontal fit
            if (barcodeWidth + margins.Horizontal > labelWidth)
                return (false, "Barcode is too wide for the label");

            // Check vertical fit (barcode + text + margins)
            double requiredHeight = barcodeHeight + textHeight + margins.Vertical;
            if (requiredHeight > labelHeight)
                return (false, "Content is too tall for the label");

            return (true, string.Empty);
        }

        /// <summary>
        /// Sanitizes input by removing invalid characters for barcode
        /// </summary>
        /// <param name="input">Input text</param>
        /// <returns>Sanitized text</returns>
        public static string SanitizeBarcodeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var sanitized = new System.Text.StringBuilder();
            
            foreach (char c in input)
            {
                // Only include valid Code 128 characters (ASCII 0-127)
                if (c <= 127)
                {
                    sanitized.Append(c);
                }
            }

            // Truncate to maximum length
            string result = sanitized.ToString();
            if (result.Length > 35)
                result = result.Substring(0, 35);

            return result;
        }

        /// <summary>
        /// Formats a double value for display with specified decimal places
        /// </summary>
        /// <param name="value">Value to format</param>
        /// <param name="decimalPlaces">Number of decimal places</param>
        /// <returns>Formatted string</returns>
        public static string FormatDouble(double value, int decimalPlaces = 1)
        {
            return Math.Round(value, decimalPlaces).ToString($"F{decimalPlaces}");
        }

        /// <summary>
        /// Checks if a string contains only printable ASCII characters
        /// </summary>
        /// <param name="text">Text to check</param>
        /// <returns>True if all characters are printable ASCII</returns>
        public static bool IsPrintableAscii(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            foreach (char c in text)
            {
                // Printable ASCII characters are from 32 (space) to 126 (~)
                if (c < 32 || c > 126)
                    return false;
            }

            return true;
        }
    }
}