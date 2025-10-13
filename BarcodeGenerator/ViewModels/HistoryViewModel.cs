using BarcodeGenerator.Models;
using BarcodeGenerator.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace BarcodeGenerator.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly ICollectionView _filteredView;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private BarcodeRecord? _selectedBarcode;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<BarcodeRecord> AllBarcodes { get; } = new();

    public ICollectionView FilteredBarcodes => _filteredView;

    public HistoryViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        
        // Create filtered view for search functionality
        _filteredView = CollectionViewSource.GetDefaultView(AllBarcodes);
        _filteredView.Filter = FilterBarcodes;
        
        // Load data on initialization
        _ = LoadBarcodesAsync();
    }

    private bool FilterBarcodes(object item)
    {
        if (item is not BarcodeRecord barcode)
            return false;

        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        var searchTerm = SearchText.ToLower();
        
        return barcode.BarcodeText.ToLower().Contains(searchTerm) ||
               (!string.IsNullOrEmpty(barcode.Description) && barcode.Description.ToLower().Contains(searchTerm)) ||
               barcode.CreatedDate.ToString("yyyy-MM-dd").Contains(searchTerm);
    }

    [RelayCommand]
    private async Task LoadBarcodesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading barcodes...";

            var barcodes = await _databaseService.GetAllBarcodeRecordsAsync();
            
            AllBarcodes.Clear();
            foreach (var barcode in barcodes.OrderByDescending(b => b.CreatedDate))
            {
                AllBarcodes.Add(barcode);
            }

            TotalCount = AllBarcodes.Count;
            StatusMessage = $"Loaded {TotalCount} barcodes";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading barcodes: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Search()
    {
        _filteredView.Refresh();
        
        var filteredCount = FilteredBarcodes.Cast<BarcodeRecord>().Count();
        StatusMessage = string.IsNullOrWhiteSpace(SearchText) 
            ? $"Showing all {TotalCount} barcodes"
            : $"Found {filteredCount} of {TotalCount} barcodes";
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        Search();
    }

    [RelayCommand(CanExecute = nameof(CanLoadSelected))]
    private void LoadSelected()
    {
        if (SelectedBarcode != null)
        {
            // Send message to MainViewModel to load this barcode
            WeakReferenceMessenger.Default.Send(new LoadBarcodeMessage(SelectedBarcode));
            StatusMessage = $"Loaded barcode: {SelectedBarcode.BarcodeText}";
        }
    }

    private bool CanLoadSelected() => SelectedBarcode != null;

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedBarcode == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Deleting barcode...";

            await _databaseService.DeleteBarcodeRecordAsync(SelectedBarcode.Id);
            AllBarcodes.Remove(SelectedBarcode);
            
            TotalCount = AllBarcodes.Count;
            StatusMessage = $"Deleted barcode. {TotalCount} remaining.";
            
            SelectedBarcode = null;
            _filteredView.Refresh();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting barcode: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanDeleteSelected() => SelectedBarcode != null && !IsLoading;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadBarcodesAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        // Automatically search when text changes (with small delay)
        Search();
    }

    partial void OnSelectedBarcodeChanged(BarcodeRecord? value)
    {
        // Update command states when selection changes
        LoadSelectedCommand.NotifyCanExecuteChanged();
        DeleteSelectedCommand.NotifyCanExecuteChanged();
    }
}