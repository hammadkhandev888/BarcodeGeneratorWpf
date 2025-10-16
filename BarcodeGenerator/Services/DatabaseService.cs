using Microsoft.EntityFrameworkCore;
using BarcodeGenerator.Data;
using BarcodeGenerator.Models;

namespace BarcodeGenerator.Services
{
    /// <summary>
    /// Service for all database operations with the 22 required methods
    /// </summary>
    public class DatabaseService : IDisposable
    {
        private readonly AppDbContext _context;

        public DatabaseService()
        {
            _context = new AppDbContext();
            // Ensure database is created on first use
            _context.EnsureCreated();
        }

        public DatabaseService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region BARCODE RECORD OPERATIONS

        /// <summary>
        /// Method 1: SaveBarcodeRecordAsync - Creates new or updates existing barcode record
        /// </summary>
        public async Task<BarcodeRecord> SaveBarcodeRecordAsync(
            string barcodeText,
            string barcodeValue,
            string description,
            int defaultLabelCount,
            double labelWidth,
            double labelHeight,
            double barcodeWidth,
            double barcodeHeight,
            string? comment = null,
            string? notes = null,
            string? filename = null)
        {
            // Search for existing record where BarcodeText matches AND IsActive = true
            var existingRecord = await _context.BarcodeRecords
                .FirstOrDefaultAsync(r => r.BarcodeText == barcodeText && r.IsActive);

            if (existingRecord != null)
            {
                // Update existing record
                existingRecord.BarcodeValue = barcodeValue;
                existingRecord.Description = description;
                existingRecord.DefaultLabelCount = defaultLabelCount;
                existingRecord.LastLabelWidth = labelWidth;
                existingRecord.LastLabelHeight = labelHeight;
                existingRecord.LastBarcodeWidth = barcodeWidth;
                existingRecord.LastBarcodeHeight = barcodeHeight;
                existingRecord.Comment = comment;
                existingRecord.Notes = notes;
                existingRecord.Filename = filename;
                existingRecord.MarkAsModified();

                await _context.SaveChangesAsync();
                return existingRecord;
            }
            else
            {
                // Create new record
                var newRecord = new BarcodeRecord
                {
                    BarcodeText = barcodeText,
                    BarcodeValue = barcodeValue,
                    Description = description,
                    DefaultLabelCount = defaultLabelCount,
                    LastLabelWidth = labelWidth,
                    LastLabelHeight = labelHeight,
                    LastBarcodeWidth = barcodeWidth,
                    LastBarcodeHeight = barcodeHeight,
                    Comment = comment,
                    Notes = notes,
                    Filename = filename,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    TotalPrintCount = 0
                };

                _context.BarcodeRecords.Add(newRecord);
                await _context.SaveChangesAsync();
                return newRecord;
            }
        }

        /// <summary>
        /// Method 2: GetBarcodeRecordAsync - Gets barcode record by ID with print histories
        /// </summary>
        public async Task<BarcodeRecord?> GetBarcodeRecordAsync(int id)
        {
            return await _context.BarcodeRecords
                .Include(r => r.PrintHistories)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// Method 3: GetBarcodeByTextAsync - Finds barcode by text
        /// </summary>
        public async Task<BarcodeRecord?> GetBarcodeByTextAsync(string barcodeText)
        {
            return await _context.BarcodeRecords
                .Include(r => r.PrintHistories)
                .FirstOrDefaultAsync(r => r.BarcodeText == barcodeText && r.IsActive);
        }

        /// <summary>
        /// Method 4: GetAllBarcodeRecordsAsync - Gets all barcode records
        /// </summary>
        public async Task<List<BarcodeRecord>> GetAllBarcodeRecordsAsync(bool includeInactive = false)
        {
            var query = _context.BarcodeRecords.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(r => r.IsActive);
            }

            return await query
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Method 5: SearchBarcodeRecordsAsync - Search barcodes by text
        /// </summary>
        public async Task<List<BarcodeRecord>> SearchBarcodeRecordsAsync(string searchText)
        {
            return await _context.BarcodeRecords
                .Where(r => r.IsActive && 
                       (r.BarcodeText.Contains(searchText) || r.Description.Contains(searchText)))
                .OrderByDescending(r => r.LastPrintedDate)
                .ThenByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Method 6: UpdateBarcodeRecordAsync - Updates existing record
        /// </summary>
        public async Task<bool> UpdateBarcodeRecordAsync(BarcodeRecord record)
        {
            try
            {
                _context.BarcodeRecords.Update(record);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Method 7: DeleteBarcodeRecordAsync - Soft delete (sets IsActive = false)
        /// </summary>
        public async Task<bool> DeleteBarcodeRecordAsync(int id)
        {
            var record = await _context.BarcodeRecords.FindAsync(id);
            if (record == null) return false;

            record.SoftDelete();
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Method 8: HardDeleteBarcodeRecordAsync - Permanent delete
        /// </summary>
        public async Task<bool> HardDeleteBarcodeRecordAsync(int id)
        {
            var record = await _context.BarcodeRecords.FindAsync(id);
            if (record == null) return false;

            _context.BarcodeRecords.Remove(record);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Method 9: GetRecentBarcodesAsync - Gets recently created barcodes
        /// </summary>
        public async Task<List<BarcodeRecord>> GetRecentBarcodesAsync(int count = 20)
        {
            return await _context.BarcodeRecords
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Method 10: GetRecentlyUsedBarcodesAsync - Gets recently printed barcodes
        /// </summary>
        public async Task<List<BarcodeRecord>> GetRecentlyUsedBarcodesAsync()
        {
            return await _context.BarcodeRecords
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Method 11: GetMostPrintedBarcodesAsync - Gets most frequently printed barcodes
        /// </summary>
        public async Task<List<BarcodeRecord>> GetMostPrintedBarcodesAsync()
        {
            return await _context.BarcodeRecords
                .Where(r => r.IsActive && r.TotalPrintCount > 0)
                .OrderByDescending(r => r.TotalPrintCount)
                .ToListAsync();
        }

        #endregion

        #region PRINT HISTORY OPERATIONS

        /// <summary>
        /// Method 12: LogPrintHistoryAsync - Records a print job
        /// </summary>
        public async Task<PrintHistory> LogPrintHistoryAsync(
            int barcodeRecordId,
            int quantityPrinted,
            string printerName,
            double labelWidth,
            double labelHeight,
            double barcodeWidth,
            double barcodeHeight,
            bool success,
            string? errorMessage = null)
        {
            var printHistory = new PrintHistory
            {
                BarcodeRecordId = barcodeRecordId,
                QuantityPrinted = quantityPrinted,
                PrinterName = printerName,
                LabelWidth = labelWidth,
                LabelHeight = labelHeight,
                BarcodeWidth = barcodeWidth,
                BarcodeHeight = barcodeHeight,
                Success = success,
                ErrorMessage = errorMessage,
                PrintedDate = DateTime.Now,
                UserName = Environment.UserName
            };

            _context.PrintHistories.Add(printHistory);

            // If successful, update the BarcodeRecord
            if (success)
            {
                var barcodeRecord = await _context.BarcodeRecords.FindAsync(barcodeRecordId);
                if (barcodeRecord != null)
                {
                    barcodeRecord.MarkAsPrinted(quantityPrinted);
                    barcodeRecord.UpdateLastUsedSettings(labelWidth, labelHeight, barcodeWidth, barcodeHeight, quantityPrinted);
                }
            }

            await _context.SaveChangesAsync();
            return printHistory;
        }

        /// <summary>
        /// Method 13: GetPrintHistoryAsync - Gets print history for a specific barcode
        /// </summary>
        public async Task<List<PrintHistory>> GetPrintHistoryAsync(int barcodeRecordId)
        {
            return await _context.PrintHistories
                .Where(h => h.BarcodeRecordId == barcodeRecordId)
                .OrderByDescending(h => h.PrintedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Method 14: GetAllPrintHistoryAsync - Gets recent print history across all barcodes
        /// </summary>
        public async Task<List<PrintHistory>> GetAllPrintHistoryAsync(int count = 50)
        {
            return await _context.PrintHistories
                .Include(h => h.BarcodeRecord)
                .OrderByDescending(h => h.PrintedDate)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Method 15: GetPrintHistoryByDateRangeAsync - Gets prints within date range
        /// </summary>
        public async Task<List<PrintHistory>> GetPrintHistoryByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.PrintHistories
                .Include(h => h.BarcodeRecord)
                .Where(h => h.PrintedDate >= startDate && h.PrintedDate <= endDate)
                .OrderByDescending(h => h.PrintedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Method 16: GetStatisticsAsync - Gets dashboard statistics
        /// </summary>
        public async Task<(int TotalBarcodes, int TotalPrints, int TodayPrints)> GetStatisticsAsync()
        {
            var totalBarcodes = await _context.BarcodeRecords.CountAsync(r => r.IsActive);
            var totalPrints = await _context.PrintHistories.SumAsync(h => h.QuantityPrinted);
            
            var today = DateTime.Today;
            var todayPrints = await _context.PrintHistories
                .Where(h => h.PrintedDate >= today)
                .SumAsync(h => h.QuantityPrinted);

            return (totalBarcodes, totalPrints, todayPrints);
        }

        /// <summary>
        /// Method 17: GetStatisticsByDateRangeAsync - Gets statistics for date range
        /// </summary>
        public async Task<(int TotalPrints, int UniqueBarcodes, double SuccessRate)> GetStatisticsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var printHistories = await _context.PrintHistories
                .Where(h => h.PrintedDate >= startDate && h.PrintedDate <= endDate)
                .ToListAsync();

            var totalPrints = printHistories.Sum(h => h.QuantityPrinted);
            var uniqueBarcodes = printHistories.Select(h => h.BarcodeRecordId).Distinct().Count();
            var successRate = printHistories.Count > 0 
                ? (double)printHistories.Count(h => h.Success) / printHistories.Count * 100 
                : 0;

            return (totalPrints, uniqueBarcodes, successRate);
        }

        #endregion

        #region LABEL TEMPLATE OPERATIONS

        /// <summary>
        /// Method 18: GetAllTemplatesAsync - Gets all label templates
        /// </summary>
        public async Task<List<LabelTemplate>> GetAllTemplatesAsync()
        {
            return await _context.LabelTemplates
                .OrderByDescending(t => t.IsDefault)
                .ThenBy(t => t.TemplateName)
                .ToListAsync();
        }

        /// <summary>
        /// Method 19: GetDefaultTemplateAsync - Gets the default template
        /// </summary>
        public async Task<LabelTemplate?> GetDefaultTemplateAsync()
        {
            return await _context.LabelTemplates
                .FirstOrDefaultAsync(t => t.IsDefault);
        }

        /// <summary>
        /// Method 20: SaveTemplateAsync - Creates new template
        /// </summary>
        public async Task<LabelTemplate> SaveTemplateAsync(
            string templateName,
            double labelWidth,
            double labelHeight,
            double barcodeWidth,
            double barcodeHeight,
            bool isDefault)
        {
            // If setting as default, remove default from all others
            if (isDefault)
            {
                var existingTemplates = await _context.LabelTemplates.ToListAsync();
                foreach (var template in existingTemplates)
                {
                    template.RemoveDefault();
                }
            }

            var newTemplate = LabelTemplate.Create(
                templateName, labelWidth, labelHeight, 
                barcodeWidth, barcodeHeight, isDefault);

            _context.LabelTemplates.Add(newTemplate);
            await _context.SaveChangesAsync();
            return newTemplate;
        }

        /// <summary>
        /// Method 21: DeleteTemplateAsync - Deletes template
        /// </summary>
        public async Task<bool> DeleteTemplateAsync(int id)
        {
            var template = await _context.LabelTemplates.FindAsync(id);
            if (template == null) return false;

            // Don't allow deletion of default template
            if (template.IsDefault) return false;

            _context.LabelTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Method 22: SetDefaultTemplateAsync - Sets template as default
        /// </summary>
        public async Task<bool> SetDefaultTemplateAsync(int id)
        {
            // Remove default from all templates
            var allTemplates = await _context.LabelTemplates.ToListAsync();
            foreach (var template in allTemplates)
            {
                template.RemoveDefault();
            }

            // Set the specified template as default
            var targetTemplate = await _context.LabelTemplates.FindAsync(id);
            if (targetTemplate == null) return false;

            targetTemplate.SetAsDefault();
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region UTILITY METHODS

        /// <summary>
        /// Gets connection string for external tools
        /// </summary>
        public string GetConnectionString()
        {
            return _context.GetConnectionString();
        }

        /// <summary>
        /// Backs up the database
        /// </summary>
        public bool BackupDatabase(string backupPath)
        {
            return _context.BackupDatabase(backupPath);
        }

        /// <summary>
        /// Gets database file size
        /// </summary>
        public long GetDatabaseSize()
        {
            return _context.GetDatabaseSize();
        }

        /// <summary>
        /// Checks database health
        /// </summary>
        public async Task<bool> CheckDatabaseHealthAsync()
        {
            try
            {
                // Try to execute a simple query
                await _context.BarcodeRecords.CountAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region DISPOSAL

        /// <summary>
        /// Disposes the database context
        /// </summary>
        public void Dispose()
        {
            _context?.Dispose();
        }

        #endregion
    }
}