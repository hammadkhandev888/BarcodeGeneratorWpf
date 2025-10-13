using System.Runtime.InteropServices;
using System.Text;

namespace BarcodeGenerator.Helpers
{
    /// <summary>
    /// Helper class for sending raw data directly to printers using Windows API
    /// </summary>
    public static class RawPrinterHelper
    {
        #region Windows API Declarations

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string? pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string? pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string? pDataType;
        }

        [DllImport("winspool.drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        #endregion

        /// <summary>
        /// Sends raw bytes to the specified printer
        /// </summary>
        /// <param name="printerName">Name of the printer</param>
        /// <param name="bytes">Byte data to send</param>
        /// <param name="documentName">Name of the print job</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool SendBytesToPrinter(string printerName, byte[] bytes, string documentName = "Barcode Label")
        {
            if (string.IsNullOrWhiteSpace(printerName))
                throw new ArgumentException("Printer name cannot be empty", nameof(printerName));

            if (bytes == null || bytes.Length == 0)
                throw new ArgumentException("Bytes cannot be null or empty", nameof(bytes));

            IntPtr hPrinter = IntPtr.Zero;
            bool success = false;

            try
            {
                // Open printer
                if (!OpenPrinter(printerName.Normalize(), out hPrinter, IntPtr.Zero))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"Failed to open printer '{printerName}'. Error code: {error}");
                }

                // Set up document info
                var docInfo = new DOCINFOA
                {
                    pDocName = documentName,
                    pDataType = "RAW"
                };

                // Start document
                if (!StartDocPrinter(hPrinter, 1, docInfo))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"Failed to start document. Error code: {error}");
                }

                try
                {
                    // Start page
                    if (!StartPagePrinter(hPrinter))
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw new InvalidOperationException($"Failed to start page. Error code: {error}");
                    }

                    try
                    {
                        // Send data to printer
                        IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
                        try
                        {
                            Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length);
                            
                            success = WritePrinter(hPrinter, pUnmanagedBytes, bytes.Length, out int bytesWritten);
                            
                            if (!success)
                            {
                                int error = Marshal.GetLastWin32Error();
                                throw new InvalidOperationException($"Failed to write to printer. Error code: {error}");
                            }

                            if (bytesWritten != bytes.Length)
                            {
                                throw new InvalidOperationException($"Not all bytes were written. Expected: {bytes.Length}, Written: {bytesWritten}");
                            }
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(pUnmanagedBytes);
                        }
                    }
                    finally
                    {
                        EndPagePrinter(hPrinter);
                    }
                }
                finally
                {
                    EndDocPrinter(hPrinter);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (in a real application, use proper logging)
                System.Diagnostics.Debug.WriteLine($"Print error: {ex.Message}");
                throw;
            }
            finally
            {
                if (hPrinter != IntPtr.Zero)
                {
                    ClosePrinter(hPrinter);
                }
            }
        }

        /// <summary>
        /// Sends a string to the printer (converts to UTF-8 bytes)
        /// </summary>
        /// <param name="printerName">Name of the printer</param>
        /// <param name="text">Text to send</param>
        /// <param name="documentName">Name of the print job</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool SendStringToPrinter(string printerName, string text, string documentName = "Barcode Label")
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Text cannot be null or empty", nameof(text));

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return SendBytesToPrinter(printerName, bytes, documentName);
        }

        /// <summary>
        /// Checks if a printer exists and is accessible
        /// </summary>
        /// <param name="printerName">Name of the printer to check</param>
        /// <returns>True if printer is accessible</returns>
        public static bool IsPrinterAccessible(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return false;

            IntPtr hPrinter = IntPtr.Zero;
            
            try
            {
                bool result = OpenPrinter(printerName.Normalize(), out hPrinter, IntPtr.Zero);
                return result;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (hPrinter != IntPtr.Zero)
                {
                    ClosePrinter(hPrinter);
                }
            }
        }

        /// <summary>
        /// Gets the last Windows error message for printer operations
        /// </summary>
        /// <returns>Error message string</returns>
        public static string GetLastErrorMessage()
        {
            int errorCode = Marshal.GetLastWin32Error();
            return GetErrorMessage(errorCode);
        }

        /// <summary>
        /// Converts Windows error code to readable message
        /// </summary>
        /// <param name="errorCode">Windows error code</param>
        /// <returns>Error message string</returns>
        public static string GetErrorMessage(int errorCode)
        {
            return errorCode switch
            {
                0 => "Success",
                2 => "The system cannot find the file specified. (Printer not found)",
                5 => "Access is denied. (Insufficient permissions)",
                87 => "The parameter is incorrect.",
                1801 => "The printer name is invalid.",
                1930 => "The printer driver is not compatible with your computer.",
                _ => $"Windows error code: {errorCode}"
            };
        }

        /// <summary>
        /// Sends a test page to the printer
        /// </summary>
        /// <param name="printerName">Name of the printer</param>
        /// <returns>True if test page was sent successfully</returns>
        public static bool SendTestPage(string printerName)
        {
            string testZpl = $@"^XA
^PW812
^LL400
^FO50,50
^A0N,50,50
^FDTEST PAGE^FS
^FO50,150
^BCN,100,Y,N,N
^FDTEST123^FS
^FO50,300
^A0N,30,30
^FDPrinter: {printerName}^FS
^XZ";

            try
            {
                return SendStringToPrinter(printerName, testZpl, "Test Page");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sends raw ZPL command with error handling and retry logic
        /// </summary>
        /// <param name="printerName">Name of the printer</param>
        /// <param name="zplCommand">ZPL command string</param>
        /// <param name="retryCount">Number of retry attempts</param>
        /// <param name="documentName">Name of the print job</param>
        /// <returns>Result with success status and error message</returns>
        public static (bool Success, string ErrorMessage) SendZplCommand(string printerName, string zplCommand, int retryCount = 3, string documentName = "Barcode Label")
        {
            for (int attempt = 0; attempt < retryCount; attempt++)
            {
                try
                {
                    bool success = SendStringToPrinter(printerName, zplCommand, documentName);
                    if (success)
                        return (true, string.Empty);
                }
                catch (Exception ex)
                {
                    if (attempt == retryCount - 1) // Last attempt
                        return (false, ex.Message);
                    
                    // Wait before retry
                    System.Threading.Thread.Sleep(1000);
                }
            }

            return (false, "Failed after all retry attempts");
        }

        /// <summary>
        /// Gets printer capabilities by attempting to open the printer
        /// </summary>
        /// <param name="printerName">Name of the printer</param>
        /// <returns>Basic printer information</returns>
        public static (bool IsAvailable, string Status) GetPrinterInfo(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return (false, "Printer name is empty");

            if (!IsPrinterAccessible(printerName))
                return (false, "Printer is not accessible or does not exist");

            return (true, "Printer is available");
        }

        /// <summary>
        /// Validates ZPL command before sending to printer
        /// </summary>
        /// <param name="zplCommand">ZPL command to validate</param>
        /// <returns>Validation result</returns>
        public static (bool IsValid, string ErrorMessage) ValidateZplCommand(string zplCommand)
        {
            if (string.IsNullOrWhiteSpace(zplCommand))
                return (false, "ZPL command is empty");

            var trimmed = zplCommand.Trim();
            
            if (!trimmed.StartsWith("^XA"))
                return (false, "ZPL command must start with ^XA");

            if (!trimmed.Contains("^XZ"))
                return (false, "ZPL command must end with ^XZ");

            return (true, string.Empty);
        }
    }
}