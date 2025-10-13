using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using BarcodeGenerator.ViewModels;
using BarcodeGenerator.Services;

namespace BarcodeGenerator.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        InitializeViewModel();
    }

    private void InitializeViewModel()
    {
        try
        {
            // Create services
            var barcodeGenerator = new BarcodeGeneratorService();
            var printerService = new PrinterService(barcodeGenerator);
            var settingsService = new SettingsService();
            var databaseService = new DatabaseService();

            // Create and set view model
            _viewModel = new MainViewModel(barcodeGenerator, printerService, settingsService, databaseService);
            DataContext = _viewModel;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize application: {ex.Message}", 
                          "Initialization Error", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Error);
            Close();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        try
        {
            _viewModel?.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
        }
        
        base.OnClosed(e);
    }

    private void PreviewScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        // Handle zoom with Ctrl+Mouse Wheel for preview
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                // Manual zoom implementation since ZoomIn/ZoomOut don't exist
                var transform = scrollViewer.RenderTransform as System.Windows.Media.ScaleTransform ?? new ScaleTransform();
                
                double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
                double newScaleX = Math.Max(0.1, Math.Min(5.0, transform.ScaleX * zoomFactor));
                double newScaleY = Math.Max(0.1, Math.Min(5.0, transform.ScaleY * zoomFactor));
                
                scrollViewer.RenderTransform = new System.Windows.Media.ScaleTransform(newScaleX, newScaleY);
                scrollViewer.RenderTransformOrigin = new Point(0.5, 0.5);
                
                e.Handled = true;
            }
        }
    }
}