using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace BarcodeGenerator.Helpers
{
    /// <summary>
    /// Helper methods for image conversion between System.Drawing and WPF
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// Converts a System.Drawing.Image to WPF BitmapSource
        /// </summary>
        /// <param name="image">The image to convert</param>
        /// <returns>BitmapSource for WPF binding</returns>
        public static BitmapSource ConvertToBitmapSource(Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

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
                    bitmapImage.Freeze();
                    
                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert image to BitmapSource: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Converts a BitmapSource back to System.Drawing.Bitmap
        /// </summary>
        /// <param name="bitmapSource">The BitmapSource to convert</param>
        /// <returns>System.Drawing.Bitmap</returns>
        public static Bitmap ConvertToBitmap(BitmapSource bitmapSource)
        {
            if (bitmapSource == null)
                throw new ArgumentNullException(nameof(bitmapSource));

            try
            {
                using (var outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new PngBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bitmapSource));
                    enc.Save(outStream);
                    
                    var bitmap = new Bitmap(outStream);
                    return new Bitmap(bitmap); // Create a copy to avoid disposal issues
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert BitmapSource to Bitmap: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Resizes an image while maintaining aspect ratio
        /// </summary>
        /// <param name="image">The image to resize</param>
        /// <param name="maxWidth">Maximum width</param>
        /// <param name="maxHeight">Maximum height</param>
        /// <param name="maintainAspectRatio">Whether to maintain aspect ratio</param>
        /// <returns>Resized image</returns>
        public static Image ResizeImage(Image image, int maxWidth, int maxHeight, bool maintainAspectRatio = true)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (maxWidth <= 0 || maxHeight <= 0)
                throw new ArgumentException("Width and height must be positive values");

            int newWidth, newHeight;

            if (maintainAspectRatio)
            {
                // Calculate new dimensions maintaining aspect ratio
                double ratioX = (double)maxWidth / image.Width;
                double ratioY = (double)maxHeight / image.Height;
                double ratio = Math.Min(ratioX, ratioY);

                newWidth = (int)(image.Width * ratio);
                newHeight = (int)(image.Height * ratio);
            }
            else
            {
                newWidth = maxWidth;
                newHeight = maxHeight;
            }

            var resizedImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(resizedImage))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }

        /// <summary>
        /// Creates a label image with barcode and description text
        /// </summary>
        /// <param name="barcodeImage">The barcode image</param>
        /// <param name="description">Description text</param>
        /// <param name="labelWidth">Label width in pixels</param>
        /// <param name="labelHeight">Label height in pixels</param>
        /// <param name="font">Font for description text</param>
        /// <param name="backgroundColor">Background color</param>
        /// <param name="textColor">Text color</param>
        /// <param name="showBorder">Whether to show border</param>
        /// <returns>Complete label image</returns>
        public static Image CreateLabelImage(Image barcodeImage, string description, 
            int labelWidth, int labelHeight, Font font, 
            Color? backgroundColor = null, Color? textColor = null, bool showBorder = false)
        {
            if (barcodeImage == null)
                throw new ArgumentNullException(nameof(barcodeImage));

            var bgColor = backgroundColor ?? Color.White;
            var txtColor = textColor ?? Color.Black;

            var labelBitmap = new Bitmap(labelWidth, labelHeight);
            
            using (var graphics = Graphics.FromImage(labelBitmap))
            {
                // Set high quality rendering
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Fill background
                graphics.Clear(bgColor);

                // Draw border if requested
                if (showBorder)
                {
                    using (var borderPen = new Pen(Color.LightGray, 1))
                    {
                        graphics.DrawRectangle(borderPen, 0, 0, labelWidth - 1, labelHeight - 1);
                    }
                }

                // Calculate barcode position (centered horizontally)
                int barcodeX = (labelWidth - barcodeImage.Width) / 2;
                int barcodeY = 20; // Top margin

                // Ensure barcode fits within label
                if (barcodeX >= 0 && barcodeY >= 0 && 
                    barcodeX + barcodeImage.Width <= labelWidth && 
                    barcodeY + barcodeImage.Height <= labelHeight)
                {
                    graphics.DrawImage(barcodeImage, barcodeX, barcodeY);

                    // Draw description text if provided
                    if (!string.IsNullOrWhiteSpace(description) && font != null)
                    {
                        using (var brush = new SolidBrush(txtColor))
                        {
                            var textSize = graphics.MeasureString(description, font);
                            float textX = (labelWidth - textSize.Width) / 2;
                            float textY = barcodeY + barcodeImage.Height + 10; // Gap below barcode

                            // Ensure text fits within label
                            if (textY + textSize.Height <= labelHeight - 10) // Bottom margin
                            {
                                graphics.DrawString(description, font, brush, textX, textY);
                            }
                        }
                    }
                }
            }

            return labelBitmap;
        }

        /// <summary>
        /// Creates a placeholder image for preview when no valid data is available
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="message">Message to display</param>
        /// <returns>Placeholder image</returns>
        public static Image CreatePlaceholderImage(int width, int height, string message = "No Preview Available")
        {
            var placeholder = new Bitmap(width, height);
            
            using (var graphics = Graphics.FromImage(placeholder))
            {
                graphics.Clear(Color.White);
                
                // Draw border
                using (var borderPen = new Pen(Color.Gray, 2))
                {
                    graphics.DrawRectangle(borderPen, 1, 1, width - 3, height - 3);
                }

                // Draw message
                using (var font = new Font("Arial", 12, FontStyle.Regular))
                using (var brush = new SolidBrush(Color.Gray))
                {
                    var textSize = graphics.MeasureString(message, font);
                    float x = (width - textSize.Width) / 2;
                    float y = (height - textSize.Height) / 2;
                    
                    graphics.DrawString(message, font, brush, x, y);
                }
            }

            return placeholder;
        }

        /// <summary>
        /// Saves an image to a byte array in the specified format
        /// </summary>
        /// <param name="image">Image to save</param>
        /// <param name="format">Image format</param>
        /// <returns>Image as byte array</returns>
        public static byte[] ImageToByteArray(Image image, ImageFormat format)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            using (var stream = new MemoryStream())
            {
                image.Save(stream, format);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Creates an image from a byte array
        /// </summary>
        /// <param name="byteArray">Image data as byte array</param>
        /// <returns>Image object</returns>
        public static Image ByteArrayToImage(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
                throw new ArgumentException("Byte array cannot be null or empty", nameof(byteArray));

            using (var stream = new MemoryStream(byteArray))
            {
                return new Bitmap(stream);
            }
        }

        /// <summary>
        /// Gets the DPI of an image
        /// </summary>
        /// <param name="image">The image</param>
        /// <returns>Tuple of (horizontal DPI, vertical DPI)</returns>
        public static (float HorizontalDpi, float VerticalDpi) GetImageDpi(Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return (image.HorizontalResolution, image.VerticalResolution);
        }

        /// <summary>
        /// Scales an image to a specific DPI
        /// </summary>
        /// <param name="image">Source image</param>
        /// <param name="targetDpi">Target DPI</param>
        /// <returns>Scaled image</returns>
        public static Image ScaleImageToDpi(Image image, float targetDpi)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (targetDpi <= 0)
                throw new ArgumentException("Target DPI must be positive", nameof(targetDpi));

            // Calculate scale factor based on current DPI
            float scaleX = targetDpi / image.HorizontalResolution;
            float scaleY = targetDpi / image.VerticalResolution;

            int newWidth = (int)(image.Width * scaleX);
            int newHeight = (int)(image.Height * scaleY);

            var scaledImage = new Bitmap(newWidth, newHeight);
            scaledImage.SetResolution(targetDpi, targetDpi);

            using (var graphics = Graphics.FromImage(scaledImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return scaledImage;
        }
    }
}