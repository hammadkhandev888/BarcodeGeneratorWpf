using BarcodeGenerator.Services;
using BarcodeGenerator.ViewModels;
using System.Windows;

namespace BarcodeGenerator.Views;

public partial class HistoryWindow : Window
{
    public HistoryWindow()
    {
        InitializeComponent();
        InitializeViewModel();
    }

    public HistoryWindow(DatabaseService databaseService) : this()
    {
        // Override the default DataContext with properly injected dependencies
        DataContext = new HistoryViewModel(databaseService);
    }

    private void InitializeViewModel()
    {
        // If no DataContext is set (default constructor), create one with new DatabaseService
        if (DataContext is not HistoryViewModel)
        {
            var databaseService = new DatabaseService();
            DataContext = new HistoryViewModel(databaseService);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}