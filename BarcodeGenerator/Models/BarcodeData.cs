using System.ComponentModel.DataAnnotations;

namespace BarcodeGenerator.Models
{
    /// <summary>
    /// Model representing barcode data and content
    /// </summary>
    public class BarcodeData
    {
        /// <summary>
        /// The string data to be encoded in the barcode (max 35 characters for Code 128)
        /// </summary>
        [Required]
        [StringLength(35, ErrorMessage = "Barcode data cannot exceed 35 characters")]
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// Description text to be printed below the barcode on the label
        /// </summary>
        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Number of copies to print
        /// </summary>
        [Range(1, 999, ErrorMessage = "Number of copies must be between 1 and 999")]
        public int Copies { get; set; } = 1;

        /// <summary>
        /// Validates if the barcode data is valid for Code 128
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValid()
        {
            // Check if data is not empty and within character limit
            if (string.IsNullOrWhiteSpace(Data) || Data.Length > 35)
                return false;

            // Check if all characters are valid for Code 128 (ASCII 0-127)
            foreach (char c in Data)
            {
                if (c > 127)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets validation error message if data is invalid
        /// </summary>
        /// <returns>Error message or empty string if valid</returns>
        public string GetValidationError()
        {
            if (string.IsNullOrWhiteSpace(Data))
                return "Barcode data is required";
            
            if (Data.Length > 35)
                return "Barcode data cannot exceed 35 characters";
            
            foreach (char c in Data)
            {
                if (c > 127)
                    return "Barcode data contains invalid characters";
            }
            
            return string.Empty;
        }
    }
}