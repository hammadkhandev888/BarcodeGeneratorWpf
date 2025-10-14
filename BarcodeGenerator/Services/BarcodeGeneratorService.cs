using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using BarcodeGenerator.Models;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace BarcodeGenerator.Services
{
    /// <summary>
    /// Service for generating Code 128 barcodes using ZXing.Net
    /// </summary>
    public class BarcodeGeneratorService
    {
        private readonly BarcodeWriter _barcodeWriter;

        public BarcodeGeneratorService()
        {
            _barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Width = 300,
                    Height = 100,
                    Margin = 10,
                    PureBarcode = true  // Don't include text below barcode
                }
            };
        }

        /// <summary>
        /// Generates a Code 128 barcode image
        /// </summary>
        /// <param name="data">Barcode text data (max 35 characters)</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <returns>Bitmap image of the barcode</returns>
        public Bitmap GenerateCode128Barcode(string data, int width, int height)
        {
            try
            {
                // Validate barcode data
                var (isValid, errorMessage) = ValidateBarcodeData(data);
                if (!isValid)
                {
                    throw new ArgumentException(errorMessage);
                }

                // Update barcode writer options with specific dimensions
                _barcodeWriter.Options.Width = width;
                _barcodeWriter.Options.Height = height;

                // Generate barcode
                var bitmap = _barcodeWriter.Write(data);
                return bitmap;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Barcode generation failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generates a Code 128 barcode with description text below
        /// </summary>
        /// <param name="data">Barcode text data</param>
        /// <param name="description">Description text to display below barcode</param>
        /// <param name="barcodeWidth">Barcode width in pixels</param>
        /// <param name="barcodeHeight">Barcode height in pixels</param>
        /// <returns>Complete label image with barcode and text</returns>
        public Image GenerateCode128BarcodeWithLabel(string data, string description, int barcodeWidth, int barcodeHeight, int descriptionFontSize = 18)
        {
            try
            {
                // Generate barcode
                var barcodeImage = GenerateCode128Barcode(data, barcodeWidth, barcodeHeight);

                // Calculate total image dimensions
                int textHeight = string.IsNullOrEmpty(description) ? 0 : 40;
                int totalWidth = Math.Max(barcodeWidth, 300); // Ensure minimum width for text
                int totalHeight = barcodeHeight + textHeight + 20; // 20px spacing

                // Create composite image
                var labelBitmap = new Bitmap(totalWidth, totalHeight);
                using (var graphics = Graphics.FromImage(labelBitmap))
                {
                    // Clear background
                    graphics.Clear(Color.White);

                    // Draw barcode centered
                    int barcodeX = (totalWidth - barcodeImage.Width) / 2;
                    graphics.DrawImage(barcodeImage, barcodeX, 10);

                    // Draw description text if provided
                    if (!string.IsNullOrEmpty(description))
                    {
                        using (var font = new Font("Arial", descriptionFontSize, FontStyle.Regular))
                        {
                            var textSize = graphics.MeasureString(description, font);
                            float textX = (totalWidth - textSize.Width) / 2;
                            float textY = barcodeHeight + 15;
                            
                            graphics.DrawString(description, font, Brushes.Black, textX, textY);
                        }
                    }
                }

                // Dispose barcode image
                barcodeImage.Dispose();

                return labelBitmap;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Label generation failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Converts System.Drawing.Image to WPF BitmapSource
        /// </summary>
        /// <param name="image">Source image</param>
        /// <returns>BitmapSource for WPF binding</returns>
        public BitmapSource ConvertToBitmapSource(Image image)
        {
            try
            {
                using (var memory = new MemoryStream())
                {
                    image.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); // For cross-thread access

                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Image conversion failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates barcode data for Code 128
        /// </summary>
        /// <param name="data">Barcode text to validate</param>
        /// <returns>Validation result with error message if invalid</returns>
        public (bool IsValid, string ErrorMessage) ValidateBarcodeData(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return (false, "Barcode text cannot be empty");

            if (data.Length > 35)
                return (false, "Barcode text cannot exceed 35 characters");

            // Check for invalid characters (Code 128 supports ASCII 0-127)
            foreach (char c in data)
            {
                if (c > 127)
                    return (false, "Barcode text contains invalid characters. Only ASCII characters (0-127) are supported.");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Gets barcode as byte array in PNG format
        /// </summary>
        /// <param name="data">Barcode text data</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <returns>PNG byte array</returns>
        public byte[] GetBarcodeAsBytes(string data, int width, int height)
        {
            try
            {
                using (var barcodeImage = GenerateCode128Barcode(data, width, height))
                {
                    using (var memory = new MemoryStream())
                    {
                        barcodeImage.Save(memory, ImageFormat.Png);
                        return memory.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Barcode byte conversion failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a preview image for the UI with proper scaling
        /// </summary>
        /// <param name="barcodeText">Barcode text</param>
        /// <param name="description">Description text</param>
        /// <param name="labelWidthMm">Label width in millimeters</param>
        /// <param name="labelHeightMm">Label height in millimeters</param>
        /// <param name="barcodeWidthMm">Barcode width in millimeters</param>
        /// <param name="barcodeHeightMm">Barcode height in millimeters</param>
        /// <param name="dpi">DPI for conversion (96 for screen, 203 for printer)</param>
        /// <returns>Preview image scaled for display</returns>
        public Image GeneratePreviewImage(
            string barcodeValue, 
            string labelText, 
            string description, 
            double labelWidthMm, 
            double labelHeightMm, 
            double barcodeWidthMm, 
            double barcodeHeightMm,
            int labelFontSize = 15,
            int descriptionFontSize = 18,
            BarcodeGenerator.Models.LabelTextAlignment labelAlignment = BarcodeGenerator.Models.LabelTextAlignment.Center,
            BarcodeGenerator.Models.LabelTextAlignment descriptionAlignment = BarcodeGenerator.Models.LabelTextAlignment.Center,
            int dpi = 96)
        {
            try
            {
                // Calculate dynamic dimensions based on barcode value length
                double adjustedLabelWidthMm = labelWidthMm;
                double adjustedBarcodeWidthMm = barcodeWidthMm;
                
                // If barcode value is longer than 16 characters, expand dimensions to prevent blurring
                if (barcodeValue.Length > 16)
                {
                    // Calculate expansion factor based on character count
                    double expansionFactor = Math.Min(2.0, 1.0 + (barcodeValue.Length - 16) * 0.05);
                    
                    // Expand barcode width
                    adjustedBarcodeWidthMm = barcodeWidthMm * expansionFactor;
                    
                    // Expand label width to accommodate the wider barcode (with margins)
                    double minLabelWidth = adjustedBarcodeWidthMm + 20; // 20mm for margins
                    adjustedLabelWidthMm = Math.Max(labelWidthMm, minLabelWidth);
                }

                // Convert mm to pixels using adjusted dimensions
                int labelWidthPx = ConvertMmToPixels(adjustedLabelWidthMm, dpi);
                int labelHeightPx = ConvertMmToPixels(labelHeightMm, dpi);
                int barcodeWidthPx = ConvertMmToPixels(adjustedBarcodeWidthMm, dpi);
                int barcodeHeightPx = ConvertMmToPixels(barcodeHeightMm, dpi);

                // Generate barcode (only encoding the value, no visible text)
                var barcodeImage = GenerateCode128Barcode(barcodeValue, barcodeWidthPx, barcodeHeightPx);

                // Create label-sized canvas
                var labelBitmap = new Bitmap(labelWidthPx, labelHeightPx);
                using (var graphics = Graphics.FromImage(labelBitmap))
                {
                    // Clear background and add border
                    graphics.Clear(Color.White);
                    graphics.DrawRectangle(Pens.LightGray, 0, 0, labelWidthPx - 1, labelHeightPx - 1);

                    // Calculate dynamic positioning based on content
                    bool hasText = !string.IsNullOrEmpty(labelText) || !string.IsNullOrEmpty(description);
                    
                    // Center barcode on label with appropriate margins
                    int barcodeX = (labelWidthPx - barcodeImage.Width) / 2;
                    int topMargin = hasText ? 15 : Math.Max(15, (labelHeightPx - barcodeImage.Height) / 2);
                    int barcodeY = Math.Max(10, Math.Min(topMargin, labelHeightPx - barcodeImage.Height - (hasText ? 25 : 10)));

                    graphics.DrawImage(barcodeImage, barcodeX, barcodeY);

                    float currentY = barcodeY + barcodeImage.Height + 10;

                    // Draw label text below barcode (if provided)
                    if (!string.IsNullOrEmpty(labelText))
                    {
                        currentY = DrawTextWithAutoResize(graphics, labelText, labelFontSize, true, 
                            labelWidthPx, labelHeightPx, labelAlignment, currentY, 10);
                    }

                    // Draw description text below label text (if provided)
                    if (!string.IsNullOrEmpty(description))
                    {
                        currentY = DrawTextWithAutoResize(graphics, description, descriptionFontSize, false, 
                            labelWidthPx, labelHeightPx, descriptionAlignment, currentY, 10);
                    }
                }

                // Dispose barcode image
                barcodeImage.Dispose();

                return labelBitmap;
            }
            catch (Exception ex)
            {
                // Return error placeholder image
                return CreateErrorImage($"Preview Error: {ex.Message}", 400, 200);
            }
        }

        /// <summary>
        /// Converts millimeters to pixels
        /// </summary>
        /// <param name="mm">Millimeters</param>
        /// <param name="dpi">Dots per inch</param>
        /// <returns>Pixels</returns>
        public int ConvertMmToPixels(double mm, int dpi)
        {
            return (int)Math.Round((mm * dpi) / 25.4);
        }

        /// <summary>
        /// Creates an error placeholder image
        /// </summary>
        /// <param name="errorMessage">Error message to display</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <returns>Error placeholder image</returns>
        private Image CreateErrorImage(string errorMessage, int width, int height)
        {
            var errorBitmap = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(errorBitmap))
            {
                graphics.Clear(Color.LightYellow);
                graphics.DrawRectangle(Pens.Red, 0, 0, width - 1, height - 1);

                using (var font = new Font("Arial", 10, FontStyle.Regular))
                {
                    var textBounds = new RectangleF(10, 10, width - 20, height - 20);
                    graphics.DrawString(errorMessage, font, Brushes.Red, textBounds);
                }
            }
            return errorBitmap;
        }

        /// <summary>
        /// Draws text with automatic font size reduction and wrapping if needed
        /// </summary>
        private float DrawTextWithAutoResize(Graphics graphics, string text, int originalFontSize, bool isBold, 
            float containerWidth, float containerHeight, BarcodeGenerator.Models.LabelTextAlignment alignment, 
            float startY, float bottomMargin)
        {
            if (string.IsNullOrEmpty(text)) return startY;

            float availableHeight = containerHeight - startY - bottomMargin;
            if (availableHeight <= 0) return startY;

            int fontSize = originalFontSize;
            FontStyle fontStyle = isBold ? FontStyle.Bold : FontStyle.Regular;
            
            // Try progressively smaller font sizes until text fits
            for (int attempts = 0; attempts < 5 && fontSize >= 6; attempts++)
            {
                using (var font = new Font("Arial", fontSize, fontStyle))
                {
                    var textSize = graphics.MeasureString(text, font);
                    
                    // Check if text fits in available space
                    if (textSize.Height <= availableHeight)
                    {
                        // Check if text width fits, if not try word wrapping
                        if (textSize.Width <= containerWidth - 20) // 20px total margin
                        {
                            // Text fits, draw it
                            float textX = CalculateTextX(containerWidth, textSize.Width, alignment);
                            graphics.DrawString(text, font, Brushes.Black, textX, startY);
                            return startY + textSize.Height + 5;
                        }
                        else
                        {
                            // Try word wrapping
                            var wrappedLines = WrapText(graphics, text, font, containerWidth - 20);
                            float totalTextHeight = wrappedLines.Count * textSize.Height;
                            
                            if (totalTextHeight <= availableHeight)
                            {
                                // Draw wrapped text
                                float currentY = startY;
                                foreach (string line in wrappedLines)
                                {
                                    var lineSize = graphics.MeasureString(line, font);
                                    float lineX = CalculateTextX(containerWidth, lineSize.Width, alignment);
                                    graphics.DrawString(line, font, Brushes.Black, lineX, currentY);
                                    currentY += lineSize.Height;
                                }
                                return currentY + 5;
                            }
                        }
                    }
                }
                
                // Reduce font size and try again
                fontSize = Math.Max(6, fontSize - 2);
            }
            
            // If we get here, draw with minimum font size anyway (truncated if necessary)
            using (var font = new Font("Arial", 6, fontStyle))
            {
                var textSize = graphics.MeasureString(text, font);
                
                // Try to fit at least one line
                if (textSize.Height <= availableHeight)
                {
                    // Truncate text if too long
                    string displayText = text;
                    while (graphics.MeasureString(displayText + "...", font).Width > containerWidth - 20 && displayText.Length > 1)
                    {
                        displayText = displayText.Substring(0, displayText.Length - 1);
                    }
                    if (displayText.Length < text.Length)
                        displayText += "...";
                    
                    var finalSize = graphics.MeasureString(displayText, font);
                    float textX = CalculateTextX(containerWidth, finalSize.Width, alignment);
                    graphics.DrawString(displayText, font, Brushes.Black, textX, startY);
                    return startY + finalSize.Height + 5;
                }
            }
            
            return startY; // No space available
        }

        /// <summary>
        /// Wraps text into multiple lines to fit within specified width
        /// </summary>
        private List<string> WrapText(Graphics graphics, string text, Font font, float maxWidth)
        {
            var lines = new List<string>();
            var words = text.Split(' ');
            
            if (words.Length == 0) return lines;
            
            string currentLine = words[0];
            
            for (int i = 1; i < words.Length; i++)
            {
                string testLine = currentLine + " " + words[i];
                var testSize = graphics.MeasureString(testLine, font);
                
                if (testSize.Width <= maxWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    lines.Add(currentLine);
                    currentLine = words[i];
                }
            }
            
            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);
            
            return lines;
        }

        /// <summary>
        /// Calculates the X position for text based on alignment
        /// </summary>
        private float CalculateTextX(float containerWidth, float textWidth, BarcodeGenerator.Models.LabelTextAlignment alignment)
        {
            return alignment switch
            {
                BarcodeGenerator.Models.LabelTextAlignment.Left => 10f, // Small left margin
                BarcodeGenerator.Models.LabelTextAlignment.Right => containerWidth - textWidth - 10f, // Small right margin
                BarcodeGenerator.Models.LabelTextAlignment.Center => (containerWidth - textWidth) / 2f,
                _ => (containerWidth - textWidth) / 2f // Default to center
            };
        }

        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            // ZXing BarcodeWriter doesn't require explicit disposal
        }
    }
}