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
                    Margin = 10
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
        public Image GenerateCode128Barcode(string data, int width, int height)
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
        public Image GenerateCode128BarcodeWithLabel(string data, string description, int barcodeWidth, int barcodeHeight)
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
                        using (var font = new Font("Arial", 12, FontStyle.Regular))
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
            string barcodeText, 
            string description, 
            double labelWidthMm, 
            double labelHeightMm, 
            double barcodeWidthMm, 
            double barcodeHeightMm, 
            int dpi = 96)
        {
            try
            {
                // Convert mm to pixels
                int labelWidthPx = ConvertMmToPixels(labelWidthMm, dpi);
                int labelHeightPx = ConvertMmToPixels(labelHeightMm, dpi);
                int barcodeWidthPx = ConvertMmToPixels(barcodeWidthMm, dpi);
                int barcodeHeightPx = ConvertMmToPixels(barcodeHeightMm, dpi);

                // Generate barcode with label dimensions
                var barcodeImage = GenerateCode128Barcode(barcodeText, barcodeWidthPx, barcodeHeightPx);

                // Create label-sized canvas
                var labelBitmap = new Bitmap(labelWidthPx, labelHeightPx);
                using (var graphics = Graphics.FromImage(labelBitmap))
                {
                    // Clear background and add border
                    graphics.Clear(Color.White);
                    graphics.DrawRectangle(Pens.LightGray, 0, 0, labelWidthPx - 1, labelHeightPx - 1);

                    // Center barcode on label
                    int barcodeX = (labelWidthPx - barcodeImage.Width) / 2;
                    int barcodeY = 20; // Top margin

                    graphics.DrawImage(barcodeImage, barcodeX, barcodeY);

                    // Draw description text below barcode
                    if (!string.IsNullOrEmpty(description))
                    {
                        using (var font = new Font("Arial", 10, FontStyle.Regular))
                        {
                            var textSize = graphics.MeasureString(description, font);
                            float textX = (labelWidthPx - textSize.Width) / 2;
                            float textY = barcodeY + barcodeImage.Height + 10;

                            // Ensure text fits on label
                            if (textY + textSize.Height <= labelHeightPx - 10)
                            {
                                graphics.DrawString(description, font, Brushes.Black, textX, textY);
                            }
                        }
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
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            // ZXing BarcodeWriter doesn't require explicit disposal
        }
    }
}