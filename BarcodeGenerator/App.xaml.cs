using System.Configuration;
using System.Data;
using System.Windows;

namespace BarcodeGenerator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Add global exception handling
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        
        base.OnStartup(e);
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Application Error: {e.Exception.Message}\n\nStack Trace: {e.Exception.StackTrace}", 
                       "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        MessageBox.Show($"Critical Error: {ex?.Message}\n\nStack Trace: {ex?.StackTrace}", 
                       "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

