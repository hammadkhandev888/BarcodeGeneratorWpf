using System.IO;
using System.Text.Json;
using BarcodeGenerator.Models;

namespace BarcodeGenerator.Services
{
    /// <summary>
    /// Service for managing application settings persistence
    /// </summary>
    public class SettingsService
    {
        private readonly string _settingsDirectory;
        private readonly string _settingsFilePath;
        private AppSettings? _currentSettings;

        public SettingsService()
        {
            // Store settings in user's AppData folder
            _settingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BarcodeGenerator");

            _settingsFilePath = Path.Combine(_settingsDirectory, "settings.json");

            // Ensure settings directory exists
            Directory.CreateDirectory(_settingsDirectory);
        }

        /// <summary>
        /// Loads application settings from file or returns default settings
        /// </summary>
        /// <returns>Application settings</returns>
        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    
                    if (settings != null)
                    {
                        _currentSettings = settings;
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with default settings
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }

            // Return default settings if loading failed
            _currentSettings = AppSettings.CreateDefault();
            return _currentSettings;
        }

        /// <summary>
        /// Saves application settings to file
        /// </summary>
        /// <param name="settings">Settings to save</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> SaveSettingsAsync(AppSettings settings)
        {
            if (settings == null)
                return false;

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string json = JsonSerializer.Serialize(settings, options);
                await File.WriteAllTextAsync(_settingsFilePath, json);

                _currentSettings = settings;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves settings synchronously
        /// </summary>
        /// <param name="settings">Settings to save</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SaveSettings(AppSettings settings)
        {
            if (settings == null)
                return false;

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_settingsFilePath, json);

                _currentSettings = settings;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets default application settings
        /// </summary>
        /// <returns>Default settings</returns>
        public AppSettings GetDefaultSettings()
        {
            return AppSettings.CreateDefault();
        }

        /// <summary>
        /// Gets current cached settings or loads from file
        /// </summary>
        /// <returns>Current settings</returns>
        public AppSettings GetCurrentSettings()
        {
            return _currentSettings ?? LoadSettings();
        }

        /// <summary>
        /// Updates label settings and saves automatically if auto-save is enabled
        /// </summary>
        /// <param name="labelSettings">New label settings</param>
        /// <returns>True if successful</returns>
        public async Task<bool> UpdateLabelSettingsAsync(LabelSettings labelSettings)
        {
            var currentSettings = GetCurrentSettings();
            currentSettings.LabelSettings = labelSettings.Clone();

            if (currentSettings.AutoSaveSettings)
            {
                return await SaveSettingsAsync(currentSettings);
            }

            _currentSettings = currentSettings;
            return true;
        }

        /// <summary>
        /// Updates printer settings and saves automatically if auto-save is enabled
        /// </summary>
        /// <param name="printerSettings">New printer settings</param>
        /// <returns>True if successful</returns>
        public async Task<bool> UpdatePrinterSettingsAsync(PrinterSettings printerSettings)
        {
            var currentSettings = GetCurrentSettings();
            currentSettings.PrinterSettings = printerSettings;

            if (currentSettings.AutoSaveSettings)
            {
                return await SaveSettingsAsync(currentSettings);
            }

            _currentSettings = currentSettings;
            return true;
        }

        /// <summary>
        /// Updates last used barcode data
        /// </summary>
        /// <param name="barcodeData">Last barcode data</param>
        /// <returns>True if successful</returns>
        public async Task<bool> UpdateLastBarcodeDataAsync(BarcodeData barcodeData)
        {
            var currentSettings = GetCurrentSettings();
            currentSettings.LastBarcodeData = new BarcodeData
            {
                Data = barcodeData.Data,
                Description = barcodeData.Description,
                Copies = barcodeData.Copies
            };

            if (currentSettings.AutoSaveSettings)
            {
                return await SaveSettingsAsync(currentSettings);
            }

            _currentSettings = currentSettings;
            return true;
        }

        /// <summary>
        /// Updates window settings (position, size, state)
        /// </summary>
        /// <param name="windowSettings">Window settings</param>
        /// <returns>True if successful</returns>
        public async Task<bool> UpdateWindowSettingsAsync(WindowSettings windowSettings)
        {
            var currentSettings = GetCurrentSettings();
            currentSettings.WindowSettings = windowSettings;

            if (currentSettings.AutoSaveSettings)
            {
                return await SaveSettingsAsync(currentSettings);
            }

            _currentSettings = currentSettings;
            return true;
        }

        /// <summary>
        /// Resets settings to default values
        /// </summary>
        /// <param name="saveImmediately">Whether to save immediately</param>
        /// <returns>True if successful</returns>
        public async Task<bool> ResetToDefaultAsync(bool saveImmediately = true)
        {
            _currentSettings = AppSettings.CreateDefault();

            if (saveImmediately)
            {
                return await SaveSettingsAsync(_currentSettings);
            }

            return true;
        }

        /// <summary>
        /// Backs up current settings to a backup file
        /// </summary>
        /// <returns>True if successful, backup file path</returns>
        public (bool Success, string BackupPath) BackupSettings()
        {
            try
            {
                string backupFileName = $"settings_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string backupPath = Path.Combine(_settingsDirectory, backupFileName);

                if (File.Exists(_settingsFilePath))
                {
                    File.Copy(_settingsFilePath, backupPath);
                    return (true, backupPath);
                }

                return (false, string.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error backing up settings: {ex.Message}");
                return (false, string.Empty);
            }
        }

        /// <summary>
        /// Restores settings from a backup file
        /// </summary>
        /// <param name="backupFilePath">Path to backup file</param>
        /// <returns>True if successful</returns>
        public async Task<bool> RestoreFromBackupAsync(string backupFilePath)
        {
            if (!File.Exists(backupFilePath))
                return false;

            try
            {
                string json = await File.ReadAllTextAsync(backupFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);

                if (settings != null)
                {
                    return await SaveSettingsAsync(settings);
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Exports settings to a specified file
        /// </summary>
        /// <param name="exportPath">Path to export file</param>
        /// <returns>True if successful</returns>
        public async Task<bool> ExportSettingsAsync(string exportPath)
        {
            try
            {
                var currentSettings = GetCurrentSettings();
                
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string json = JsonSerializer.Serialize(currentSettings, options);
                await File.WriteAllTextAsync(exportPath, json);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Imports settings from a specified file
        /// </summary>
        /// <param name="importPath">Path to import file</param>
        /// <returns>True if successful</returns>
        public async Task<bool> ImportSettingsAsync(string importPath)
        {
            if (!File.Exists(importPath))
                return false;

            try
            {
                string json = await File.ReadAllTextAsync(importPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);

                if (settings != null)
                {
                    return await SaveSettingsAsync(settings);
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the settings file path
        /// </summary>
        /// <returns>Full path to settings file</returns>
        public string GetSettingsFilePath()
        {
            return _settingsFilePath;
        }

        /// <summary>
        /// Checks if settings file exists
        /// </summary>
        /// <returns>True if settings file exists</returns>
        public bool SettingsFileExists()
        {
            return File.Exists(_settingsFilePath);
        }

        /// <summary>
        /// Deletes the settings file (resets to default)
        /// </summary>
        /// <returns>True if successful</returns>
        public bool DeleteSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    File.Delete(_settingsFilePath);
                }

                _currentSettings = null;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validates settings integrity
        /// </summary>
        /// <param name="settings">Settings to validate</param>
        /// <returns>Validation result</returns>
        public (bool IsValid, List<string> Errors) ValidateSettings(AppSettings settings)
        {
            var errors = new List<string>();

            if (settings == null)
            {
                errors.Add("Settings object is null");
                return (false, errors);
            }

            // Validate label settings
            if (settings.LabelSettings == null)
            {
                errors.Add("Label settings are missing");
            }
            else if (!settings.LabelSettings.IsValidLayout())
            {
                errors.Add(settings.LabelSettings.GetLayoutValidationError());
            }

            // Validate printer settings
            if (settings.PrinterSettings == null)
            {
                errors.Add("Printer settings are missing");
            }

            // Validate window settings
            if (settings.WindowSettings == null)
            {
                errors.Add("Window settings are missing");
            }
            else
            {
                if (settings.WindowSettings.Width <= 0 || settings.WindowSettings.Height <= 0)
                    errors.Add("Invalid window dimensions");
            }

            return (errors.Count == 0, errors);
        }
    }
}