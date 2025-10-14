using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using BarcodeGenerator.Models;
using BarcodeGenerator.Services;
using BarcodeGenerator.Helpers;
using BarcodeGenerator.Views;

namespace BarcodeGenerator.ViewModels
{
    /// <summary>
    /// Main view model for the barcode generator application
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly BarcodeGeneratorService _barcodeGenerator;
        private readonly PrinterService _printerService;
        private readonly SettingsService _settingsService;
        private readonly DatabaseService _databaseService;
        private readonly MainViewModel? _mainViewModel;  
        private readonly DispatcherTimer _previewUpdateTimer;

        #region Observable Properties

        [ObservableProperty]
        private string _barcodeData = string.Empty;

        [ObservableProperty]
        private string _barcodeValue = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private int _copies = 1;

        [ObservableProperty]
        private double _labelWidth = 100.0;

        [ObservableProperty]
        private double _labelHeight = 60.0;

        [ObservableProperty]
        private double _barcodeWidth = 80.0;

        [ObservableProperty]
        private double _barcodeHeight = 20.0;

        private int _descriptionFontSize = 18;
        private int _labelFontSize = 15;

        /// <summary>
        /// Font size for description text
        /// </summary>
        public int DescriptionFontSize
        {
            get => _descriptionFontSize;
            set
            {
                if (SetProperty(ref _descriptionFontSize, Math.Max(1, Math.Min(32, value))))
                {
                    TriggerPreviewUpdate();
                }
            }
        }

        /// <summary>
        /// Font size for label text
        /// </summary>
        public int LabelFontSize
        {
            get => _labelFontSize;
            set
            {
                if (SetProperty(ref _labelFontSize, Math.Max(1, Math.Min(24, value))))
                {
                    TriggerPreviewUpdate();
                }
            }
        }

        [ObservableProperty]
        private BitmapSource? _previewImage;

        [ObservableProperty]
        private string _selectedPrinter = string.Empty;

        [ObservableProperty]
        private bool _isValidData = false;

        [ObservableProperty]
        private string _validationMessage = string.Empty;

        [ObservableProperty]
        private bool _isPrinting = false;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private bool _showPreview = true;

        [ObservableProperty]
        private bool _isPreviewGenerating = false;

        // Database-related properties
        [ObservableProperty]
        private BarcodeRecord? _currentBarcodeRecord;

        [ObservableProperty]
        private string? _comment;

        #endregion

        #region Collections

        public ObservableCollection<string> AvailablePrinters { get; }
        public ObservableCollection<BarcodeRecord> RecentBarcodes { get; }

        #endregion

        #region Computed Properties

        /// <summary>
        /// Gets whether a printer is selected
        /// </summary>
        public bool HasSelectedPrinter => !string.IsNullOrWhiteSpace(SelectedPrinter);

        /// <summary>
        /// Called when SelectedPrinter property changes
        /// </summary>
        partial void OnSelectedPrinterChanged(string value)
        {
            OnPropertyChanged(nameof(HasSelectedPrinter));
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task PrintLabelsAsync()
        {
            await ExecutePrintAsync();
        }



        [RelayCommand]
        private void TestZplGeneration()
        {
            try
            {
                var testData = new BarcodeData
                {
                    Data = !string.IsNullOrEmpty(BarcodeData) ? BarcodeData : "TEST123",
                    Value = !string.IsNullOrEmpty(BarcodeValue) ? BarcodeValue : "TEST123",
                    Description = !string.IsNullOrEmpty(Description) ? Description : "Test Label",
                    Copies = Copies
                };

                var testSettings = new LabelSettings
                {
                    LabelWidth = LabelWidth,
                    LabelHeight = LabelHeight,
                    BarcodeWidth = BarcodeWidth,
                    BarcodeHeight = BarcodeHeight,
                    DescriptionFontSize = 18,
                    LabelFontSize = 15
                };

                string zplCommand = ZplCommandGenerator.GenerateLabelZpl(testData, testSettings, 203);
                
                StatusMessage = $"ZPL Generated: {zplCommand.Length} characters";
                
                // Show in debug output for now
                System.Diagnostics.Debug.WriteLine("=== Generated ZPL Command ===");
                System.Diagnostics.Debug.WriteLine(zplCommand);
                System.Diagnostics.Debug.WriteLine("=============================");

                // Also test ZPL validation
                var (isValid, validationError) = RawPrinterHelper.ValidateZplCommand(zplCommand);
                if (isValid)
                {
                    StatusMessage += " - ZPL Valid ✓";
                    System.Diagnostics.Debug.WriteLine("ZPL Command is valid");
                }
                else
                {
                    StatusMessage += $" - ZPL Invalid: {validationError}";
                    System.Diagnostics.Debug.WriteLine($"ZPL Validation Failed: {validationError}");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"ZPL Generation Failed: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"ZPL Error: {ex}");
            }
        }

        [RelayCommand]
        private void TestPrinterConnection()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedPrinter))
                {
                    StatusMessage = "No printer selected";
                    return;
                }

                StatusMessage = "Testing printer connection...";

                // Test printer info retrieval (this doesn't require actual printing)
                var (isAvailable, status) = RawPrinterHelper.GetPrinterInfo(SelectedPrinter);
                
                if (isAvailable)
                {
                    StatusMessage = $"Printer '{SelectedPrinter}' is available ✓";
                    System.Diagnostics.Debug.WriteLine($"Printer Status: {status}");
                }
                else
                {
                    StatusMessage = $"Printer '{SelectedPrinter}' not available: {status}";
                    System.Diagnostics.Debug.WriteLine($"Printer Connection Failed: {status}");
                }

                // Check if it's a Zebra printer
                bool isZebra = _printerService.IsZebraPrinter(SelectedPrinter);
                if (isZebra)
                {
                    StatusMessage += " (Zebra Printer)";
                }

                System.Diagnostics.Debug.WriteLine($"Is Zebra Printer: {isZebra}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Printer test failed: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Printer Test Error: {ex}");
            }
        }

        [RelayCommand]
        private void ResetToDefaults()
        {
            ResetSettingsToDefault();
        }

        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            await SaveCurrentSettingsAsync();
        }

    [RelayCommand]
    private async Task SaveRecordAsync()
        {
            try
            {
                StatusMessage = "Saving record...";

                var saved = await _databaseService.SaveBarcodeRecordAsync(
                    BarcodeData,
                    BarcodeValue,
                    Description ?? string.Empty,
                    Copies,
                    LabelWidth,
                    LabelHeight,
                    BarcodeWidth,
                    BarcodeHeight,
                    Comment);

                // Insert saved record into RecentBarcodes (ensure no duplicate)
                var existing = RecentBarcodes.FirstOrDefault(r => r.Id == saved.Id);
                if (existing != null)
                    RecentBarcodes.Remove(existing);

                RecentBarcodes.Insert(0, saved);

                // Keep a reasonable limit (10)
                while (RecentBarcodes.Count > 10)
                    RecentBarcodes.RemoveAt(RecentBarcodes.Count - 1);

                // Clear inputs as requested
                ClearAllInputs();

                StatusMessage = $"Saved: {saved.BarcodeText}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving record: {ex.Message}";
            }
        }

    [RelayCommand]
    private async Task SavePdfAsync()
        {
            try
            {
                StatusMessage = "Saving PDF...";

                // Use WPF SaveFileDialog to get path from user
                var dlg = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    FileName = string.IsNullOrWhiteSpace(BarcodeValue) ? "barcode" : BarcodeValue,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                bool? result = dlg.ShowDialog();
                if (result != true || string.IsNullOrEmpty(dlg.FileName))
                {
                    StatusMessage = "PDF save canceled";
                    return;
                }

                // Generate the label image at printer DPI for better print quality
                using (var labelImage = _barcodeGenerator.GeneratePreviewImage(
                    BarcodeValue,
                    BarcodeData,
                    Description,
                    LabelWidth,
                    LabelHeight,
                    BarcodeWidth,
                    BarcodeHeight,
                    LabelFontSize,
                    DescriptionFontSize,
                    BarcodeGenerator.Models.LabelTextAlignment.Center,
                    BarcodeGenerator.Models.LabelTextAlignment.Center,
                    dpi: 203))
                {
                    // Convert image to PNG bytes
                    byte[] pngBytes;
                    using (var ms = new System.IO.MemoryStream())
                    {
                        labelImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        pngBytes = ms.ToArray();
                    }

                    // Create a PDF with the PNG image using PdfSharpCore
                    try
                    {
                        // Lazy load PdfSharpCore types to avoid compile-time issues if package not referenced
                        using (var pdfStream = System.IO.File.Open(dlg.FileName, System.IO.FileMode.Create))
                        {
                            var pdf = new PdfSharpCore.Pdf.PdfDocument();
                            var page = pdf.AddPage();
                            // set page size to match image
                            using (var imgStream = new System.IO.MemoryStream(pngBytes))
                            {
                                var image = PdfSharpCore.Drawing.XImage.FromStream(() => imgStream);
                                // Use image point dimensions (PDF points) to size the page
                                page.Width = PdfSharpCore.Drawing.XUnit.FromPoint(image.PointWidth);
                                page.Height = PdfSharpCore.Drawing.XUnit.FromPoint(image.PointHeight);
                                var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
                                gfx.DrawImage(image, 0, 0, page.Width, page.Height);
                                pdf.Save(pdfStream);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        StatusMessage = $"Failed to create PDF: {ex.Message}";
                        return;
                    }
                }

                // Save record to database (reuse SaveBarcodeRecordAsync)
                var saved = await _databaseService.SaveBarcodeRecordAsync(
                    BarcodeData,
                    BarcodeValue,
                    Description ?? string.Empty,
                    Copies,
                    LabelWidth,
                    LabelHeight,
                    BarcodeWidth,
                    BarcodeHeight,
                    Comment);

                // Update recent list
                var existing = RecentBarcodes.FirstOrDefault(r => r.Id == saved.Id);
                if (existing != null)
                    RecentBarcodes.Remove(existing);

                RecentBarcodes.Insert(0, saved);
                while (RecentBarcodes.Count > 10)
                    RecentBarcodes.RemoveAt(RecentBarcodes.Count - 1);

                // Clear inputs
                ClearAllInputs();

                StatusMessage = $"PDF saved and record saved: {saved.BarcodeText}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Save PDF failed: {ex.Message}";
            }
        }

        private bool CanSave() => IsValidData && !IsPrinting;

        [RelayCommand]
        private void ClearData()
        {
            ClearAllInputs();
        }

        [RelayCommand]
        private void GeneratePreview()
        {
            UpdatePreview();
        }

        [RelayCommand]
        private async Task LoadBarcode(BarcodeRecord? record)
        {
            if (record != null)
                await ExecuteLoadBarcodeAsync(record);
        }

        [RelayCommand]
        private async Task ViewHistoryAsync()
        {
            await ExecuteViewHistoryAsync();
        }

        [RelayCommand]
        private async Task RefreshHistoryAsync()
        {
            await LoadRecentBarcodesAsync();
        }

        [RelayCommand]
        private void OpenHistory()
        {
            var historyWindow = new HistoryWindow(_databaseService, _mainViewModel);
            historyWindow.Show();
        }

        #endregion

        public MainViewModel(
            BarcodeGeneratorService barcodeGenerator,
            PrinterService printerService,
            SettingsService settingsService,
            DatabaseService databaseService,
            MainViewModel? mainViewModel)
        {
            _barcodeGenerator = barcodeGenerator ?? throw new ArgumentNullException(nameof(barcodeGenerator));
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _mainViewModel = mainViewModel; // Allow null for root instance

            AvailablePrinters = new ObservableCollection<string>();
            RecentBarcodes = new ObservableCollection<BarcodeRecord>();

            // Initialize preview update timer with debouncing
            _previewUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _previewUpdateTimer.Tick += OnPreviewUpdateTimerTick;

            // Register message handlers
            WeakReferenceMessenger.Default.Register<LoadBarcodeMessage>(this, OnLoadBarcodeMessage);
            WeakReferenceMessenger.Default.Register<BarcodeDeletedMessage>(this, OnBarcodeDeletedMessage);

            // Load initial data
            InitializeAsync();
        }

        #region Initialization

        private async void InitializeAsync()
        {
            try
            {
                // Load settings
                var settings = _settingsService.LoadSettings();
                ApplySettings(settings);

                // Load available printers
                LoadAvailablePrinters();

                // Load recent barcodes from database
                await LoadRecentBarcodesAsync();

                // Set up property change handlers for validation
                PropertyChanged += OnPropertyChanged;

                // Generate initial preview
                UpdatePreview();

                StatusMessage = "Application loaded successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Initialization error: {ex.Message}";
            }
        }

        private void ApplySettings(AppSettings settings)
        {
            // Apply label settings
            LabelWidth = settings.LabelSettings.LabelWidth;
            LabelHeight = settings.LabelSettings.LabelHeight;
            BarcodeWidth = settings.LabelSettings.BarcodeWidth;
            BarcodeHeight = settings.LabelSettings.BarcodeHeight;
            DescriptionFontSize = settings.LabelSettings.DescriptionFontSize;
            LabelFontSize = settings.LabelSettings.LabelFontSize;

            // Apply last barcode data
            BarcodeData = settings.LastBarcodeData.Data;
            BarcodeValue = settings.LastBarcodeData.Value;
            Description = settings.LastBarcodeData.Description;
            Copies = settings.LastBarcodeData.Copies;

            // Apply printer settings
            SelectedPrinter = settings.PrinterSettings.SelectedPrinter;
            ShowPreview = settings.ShowPreview;
        }

        private void LoadAvailablePrinters()
        {
            try
            {
                AvailablePrinters.Clear();
                var printers = _printerService.GetAvailablePrinters();
                
                foreach (var printer in printers)
                {
                    AvailablePrinters.Add(printer);
                }

                // Select first printer if none selected
                if (string.IsNullOrEmpty(SelectedPrinter) && AvailablePrinters.Count > 0)
                {
                    SelectedPrinter = AvailablePrinters.First();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading printers: {ex.Message}";
            }
        }

        #endregion

        #region Property Changed Handlers

        partial void OnBarcodeDataChanged(string value)
        {
            ValidateInput();
            TriggerPreviewUpdate();
        }

        partial void OnBarcodeValueChanged(string value)
        {
            ValidateInput();
            TriggerPreviewUpdate();
        }

        partial void OnDescriptionChanged(string value)
        {
            TriggerPreviewUpdate();
        }

        partial void OnLabelWidthChanged(double value)
        {
            ValidateInput();
            TriggerPreviewUpdate();
        }

        partial void OnLabelHeightChanged(double value)
        {
            ValidateInput();
            TriggerPreviewUpdate();
        }

        partial void OnBarcodeWidthChanged(double value)
        {
            ValidateInput();
            TriggerPreviewUpdate();
        }

        partial void OnBarcodeHeightChanged(double value)
        {
            ValidateInput();
            TriggerPreviewUpdate();
        }

        #endregion

        #region Validation

        private void ValidateInput()
        {
            var barcodeValidation = ValidationHelper.ValidateBarcodeText(BarcodeValue);
            var labelValidation = ValidationHelper.ValidateLabelSize(LabelWidth, LabelHeight);
            var barcodeValidation2 = ValidationHelper.ValidateBarcodeSize(BarcodeWidth, BarcodeHeight);
            var layoutValidation = ValidationHelper.ValidateLayout(
                LabelWidth, LabelHeight, BarcodeWidth, BarcodeHeight,
                (10.0, 10.0)); // Default margins

            if (!barcodeValidation.IsValid)
            {
                IsValidData = false;
                ValidationMessage = barcodeValidation.ErrorMessage;
                return;
            }

            if (!labelValidation.IsValid)
            {
                IsValidData = false;
                ValidationMessage = labelValidation.ErrorMessage;
                return;
            }

            if (!barcodeValidation2.IsValid)
            {
                IsValidData = false;
                ValidationMessage = barcodeValidation2.ErrorMessage;
                return;
            }

            if (!layoutValidation.IsValid)
            {
                IsValidData = false;
                ValidationMessage = layoutValidation.ErrorMessage;
                return;
            }

            IsValidData = true;
            ValidationMessage = string.Empty;
        }

        #endregion

        #region Preview Generation

        private void TriggerPreviewUpdate()
        {
            if (!ShowPreview)
                return;

            _previewUpdateTimer.Stop();
            _previewUpdateTimer.Start();
        }

        private void OnPreviewUpdateTimerTick(object? sender, EventArgs e)
        {
            _previewUpdateTimer.Stop();
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (!ShowPreview)
                return;

            if (IsPreviewGenerating)
                return;

            IsPreviewGenerating = true;

            try
            {
                var barcodeData = GetCurrentBarcodeData();
                var labelSettings = GetCurrentLabelSettings();

                // Validate before generating
                if (!barcodeData.IsValid() || !labelSettings.IsValidLayout())
                {
                    PreviewImage = CreatePlaceholderPreview("Invalid Data");
                    return;
                }

                // Generate barcode image
                using (var labelImage = _barcodeGenerator.GeneratePreviewImage(
                    BarcodeValue, 
                    BarcodeData, 
                    Description,
                    LabelWidth, 
                    LabelHeight, 
                    BarcodeWidth, 
                    BarcodeHeight,
                    LabelFontSize,
                    DescriptionFontSize,
                    Models.LabelTextAlignment.Center, // Label alignment - default to center
                    Models.LabelTextAlignment.Center)) // Description alignment - default to center
                {
                    PreviewImage = _barcodeGenerator.ConvertToBitmapSource(labelImage);
                }
            }
            catch (Exception ex)
            {
                PreviewImage = CreatePlaceholderPreview($"Preview Error: {ex.Message}");
            }
            finally
            {
                IsPreviewGenerating = false;
            }
        }

        private BitmapSource CreatePlaceholderPreview(string message)
        {
            try
            {
                // Calculate dynamic placeholder size based on barcode value length
                int placeholderWidth = 400;
                int placeholderHeight = 200;
                
                // If barcode value is longer than 16 characters, expand placeholder
                if (!string.IsNullOrEmpty(BarcodeValue) && BarcodeValue.Length > 16)
                {
                    double expansionFactor = Math.Min(2.0, 1.0 + (BarcodeValue.Length - 16) * 0.05);
                    placeholderWidth = (int)(400 * expansionFactor);
                }
                
                using (var placeholder = ImageHelper.CreatePlaceholderImage(placeholderWidth, placeholderHeight, message))
                {
                    return ImageHelper.ConvertToBitmapSource(placeholder);
                }
            }
            catch
            {
                return new BitmapImage(); // Return empty bitmap on error
            }
        }

        #endregion

        #region Printing

        private async Task ExecutePrintAsync()
        {
            if (IsPrinting)
                return;

            try
            {
                IsPrinting = true;
                StatusMessage = "Printing...";

                var barcodeData = GetCurrentBarcodeData();
                var labelSettings = GetCurrentLabelSettings();

                // Save/update barcode record before printing
                if (CurrentBarcodeRecord == null || CurrentBarcodeRecord.BarcodeText != BarcodeData)
                {
                    CurrentBarcodeRecord = await _databaseService.SaveBarcodeRecordAsync(
                        BarcodeData,
                        BarcodeValue,
                        Description ?? string.Empty,
                        Copies,
                        LabelWidth,
                        LabelHeight,
                        BarcodeWidth,
                        BarcodeHeight,
                        Comment);
                }

                // Validate print settings
                var validation = _printerService.ValidatePrintSettings(barcodeData, labelSettings, SelectedPrinter);
                if (!validation.IsValid)
                {
                    StatusMessage = $"Print validation failed: {validation.ErrorMessage}";
                    return;
                }

                // Execute print
                var result = await _printerService.PrintLabelsAsync(barcodeData, labelSettings, SelectedPrinter);
                
                // Log print history to database
                await _databaseService.LogPrintHistoryAsync(
                    CurrentBarcodeRecord.Id,
                    Copies,
                    SelectedPrinter ?? "Unknown",
                    LabelWidth,
                    LabelHeight,
                    BarcodeWidth,
                    BarcodeHeight,
                    result.Success,
                    result.Success ? null : result.Message);

                StatusMessage = result.Success ? result.Message : $"Print failed: {result.Message}";

                // Save settings after successful print
                if (result.Success)
                {
                    await SaveCurrentSettingsAsync();
                    // Refresh recent barcodes to show updated print count
                    await LoadRecentBarcodesAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Print error: {ex.Message}";
            }
            finally
            {
                IsPrinting = false;
            }
        }

        private async Task ExecuteTestPrintAsync()
        {
            if (string.IsNullOrEmpty(SelectedPrinter))
            {
                StatusMessage = "Please select a printer first";
                return;
            }

            try
            {
                StatusMessage = "Sending test print...";
                
                var result = await _printerService.TestPrintAsync(SelectedPrinter);
                StatusMessage = result.Success ? result.Message : $"Test print failed: {result.Message}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Test print error: {ex.Message}";
            }
        }

        #endregion

        #region Settings Management

        private BarcodeData GetCurrentBarcodeData()
        {
            return new BarcodeData
            {
                Data = BarcodeData,
                Value = BarcodeValue,
                Description = Description,
                Copies = Copies
            };
        }

        private LabelSettings GetCurrentLabelSettings()
        {
            return new LabelSettings
            {
                LabelWidth = LabelWidth,
                LabelHeight = LabelHeight,
                BarcodeWidth = BarcodeWidth,
                BarcodeHeight = BarcodeHeight,
                DescriptionFontSize = DescriptionFontSize,
                LabelFontSize = LabelFontSize
            };
        }

        private PrinterSettings GetCurrentPrinterSettings()
        {
            return new PrinterSettings
            {
                SelectedPrinter = SelectedPrinter
            };
        }

        private async Task SaveCurrentSettingsAsync()
        {
            try
            {
                var settings = new AppSettings
                {
                    LabelSettings = GetCurrentLabelSettings(),
                    PrinterSettings = GetCurrentPrinterSettings(),
                    LastBarcodeData = GetCurrentBarcodeData(),
                    ShowPreview = ShowPreview
                };

                await _settingsService.SaveSettingsAsync(settings);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to save settings: {ex.Message}";
            }
        }

        private void ResetSettingsToDefault()
        {
            var defaults = _settingsService.GetDefaultSettings();
            
            // Only reset size-related properties, keep barcode data and description
            LabelWidth = defaults.LabelSettings.LabelWidth;
            LabelHeight = defaults.LabelSettings.LabelHeight;
            BarcodeWidth = defaults.LabelSettings.BarcodeWidth;
            BarcodeHeight = defaults.LabelSettings.BarcodeHeight;
            DescriptionFontSize = defaults.LabelSettings.DescriptionFontSize;
            LabelFontSize = defaults.LabelSettings.LabelFontSize;
            
            UpdatePreview();
            StatusMessage = "Label dimensions reset to default";
        }

        private void ClearAllInputs()
        {
            BarcodeData = string.Empty;
            BarcodeValue = string.Empty;
            Description = string.Empty;
            Copies = 1;
            StatusMessage = "Inputs cleared";
        }

        #endregion

        #region Utility Methods

        public bool CanPrint()
        {
            return IsValidData && !string.IsNullOrEmpty(SelectedPrinter) && !IsPrinting;
        }

        public bool CanTestPrint()
        {
            return !string.IsNullOrEmpty(SelectedPrinter) && !IsPrinting;
        }

        #endregion

        #region Database Methods

        /// <summary>
        /// Loads recent barcodes from database for the sidebar
        /// </summary>
        private async Task LoadRecentBarcodesAsync()
        {
            try
            {
                var recentBarcodes = await _databaseService.GetRecentlyUsedBarcodesAsync(10);
                RecentBarcodes.Clear();
                foreach (var barcode in recentBarcodes)
                {
                    RecentBarcodes.Add(barcode);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load recent barcodes: {ex.Message}";
            }
        }

        /// <summary>
        /// Loads barcode from database record into UI
        /// </summary>
        private Task ExecuteLoadBarcodeAsync(BarcodeRecord record)
        {
            try
            {
                
                BarcodeData = record.BarcodeText;
                Description = record.Description;
                Copies = record.DefaultLabelCount;
                BarcodeValue = record.BarcodeValue;

                // Load saved dimensions if available
                if (record.LastLabelWidth.HasValue) LabelWidth = record.LastLabelWidth.Value;
                if (record.LastLabelHeight.HasValue) LabelHeight = record.LastLabelHeight.Value;
                if (record.LastBarcodeWidth.HasValue) BarcodeWidth = record.LastBarcodeWidth.Value;
                if (record.LastBarcodeHeight.HasValue) BarcodeHeight = record.LastBarcodeHeight.Value;

                CurrentBarcodeRecord = record;
                Comment = record.Comment;

                StatusMessage = $"Loaded: {record.BarcodeText} (Default qty: {record.DefaultLabelCount})";

                // Update preview with loaded data
                UpdatePreview();
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Load failed: {ex.Message}";
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Opens history window
        /// </summary>
        private async Task ExecuteViewHistoryAsync()
        {
            try
            {
                // TODO: Create and show HistoryWindow
                StatusMessage = "History window not implemented yet";
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening history: {ex.Message}";
            }
        }

        /// <summary>
        /// Property change handler for validation
        /// </summary>
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BarcodeData):
                    // Check if barcode text changed - reset current record if different
                    if (CurrentBarcodeRecord?.BarcodeText != BarcodeData)
                    {
                        CurrentBarcodeRecord = null;
                    }
                    break;
                    
                case nameof(CurrentBarcodeRecord):
                    // CurrentBarcodeRecord changed
                    break;
            }
        }

        #endregion

        #region Message Handlers

        private void OnLoadBarcodeMessage(object recipient, LoadBarcodeMessage message)
        {
            var barcode = message.Barcode;
            
            BarcodeData = barcode.BarcodeText;
            Description = barcode.Description ?? string.Empty;
            LabelWidth = barcode.LastLabelWidth ?? LabelWidth;
            LabelHeight = barcode.LastLabelHeight ?? LabelHeight;
            BarcodeWidth = barcode.LastBarcodeWidth ?? BarcodeWidth;
            BarcodeHeight = barcode.LastBarcodeHeight ?? BarcodeHeight;
            Copies = barcode.DefaultLabelCount;
            
            CurrentBarcodeRecord = barcode;
            
            TriggerPreviewUpdate();
        }

        private async void OnBarcodeDeletedMessage(object recipient, BarcodeDeletedMessage message)
        {
            await LoadRecentBarcodesAsync();
        }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            _previewUpdateTimer?.Stop();
            _previewUpdateTimer?.Tick -= OnPreviewUpdateTimerTick;
            
            // Unregister message handlers
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }

        #endregion
    }
}

// Messages for communication with HistoryViewModel
namespace BarcodeGenerator.ViewModels
{
    public record LoadBarcodeMessage(BarcodeRecord Barcode);
    public record BarcodeDeletedMessage(int BarcodeId);
}