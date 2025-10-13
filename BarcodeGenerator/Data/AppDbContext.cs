using Microsoft.EntityFrameworkCore;
using BarcodeGenerator.Models;
using System.IO;

namespace BarcodeGenerator.Data
{
    /// <summary>
    /// Entity Framework Core database context for barcode generator application
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Database file path
        /// </summary>
        public string DbPath { get; }

        /// <summary>
        /// BarcodeRecords table
        /// </summary>
        public DbSet<BarcodeRecord> BarcodeRecords { get; set; }

        /// <summary>
        /// PrintHistories table
        /// </summary>
        public DbSet<PrintHistory> PrintHistories { get; set; }

        /// <summary>
        /// LabelTemplates table
        /// </summary>
        public DbSet<LabelTemplate> LabelTemplates { get; set; }

        public AppDbContext()
        {
            // Get LocalApplicationData folder path
            var folder = Environment.SpecialFolder.ApplicationData;
            var path = Environment.GetFolderPath(folder);
            var appFolder = Path.Combine(path, "BarcodeGenerator");
            
            // Ensure directory exists
            Directory.CreateDirectory(appFolder);
            
            // Set database file path
            DbPath = Path.Combine(appFolder, "barcodes.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // Configure SQLite
            options.UseSqlite($"Data Source={DbPath}");

#if DEBUG
            // Enable sensitive data logging and console logging in debug mode
            options.EnableSensitiveDataLogging();
            options.LogTo(Console.WriteLine);
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureBarcodeRecord(modelBuilder);
            ConfigurePrintHistory(modelBuilder);
            ConfigureLabelTemplate(modelBuilder);
            SeedData(modelBuilder);
            
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Configure BarcodeRecord entity
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        private void ConfigureBarcodeRecord(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<BarcodeRecord>();

            // Primary key
            entity.HasKey(e => e.Id);

            // Indexes for performance
            entity.HasIndex(e => e.BarcodeText)
                  .HasDatabaseName("IX_BarcodeRecords_BarcodeText");

            entity.HasIndex(e => e.CreatedDate)
                  .HasDatabaseName("IX_BarcodeRecords_CreatedDate");

            entity.HasIndex(e => e.IsActive)
                  .HasDatabaseName("IX_BarcodeRecords_IsActive");

            entity.HasIndex(e => e.LastPrintedDate)
                  .HasDatabaseName("IX_BarcodeRecords_LastPrintedDate");

            // Configure relationship to PrintHistories
            entity.HasMany(e => e.PrintHistories)
                  .WithOne(e => e.BarcodeRecord)
                  .HasForeignKey(e => e.BarcodeRecordId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Property configurations
            entity.Property(e => e.BarcodeText)
                  .IsRequired()
                  .HasMaxLength(35);

            entity.Property(e => e.Description)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(e => e.BarcodeType)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasDefaultValue("CODE128");

            entity.Property(e => e.CreatedDate)
                  .IsRequired()
                  .HasDefaultValueSql("datetime('now')");

            entity.Property(e => e.TotalPrintCount)
                  .IsRequired()
                  .HasDefaultValue(0);

            entity.Property(e => e.DefaultLabelCount)
                  .IsRequired()
                  .HasDefaultValue(1);

            entity.Property(e => e.IsActive)
                  .IsRequired()
                  .HasDefaultValue(true);

            entity.Property(e => e.IsExported)
                  .IsRequired()
                  .HasDefaultValue(false);

            entity.Property(e => e.Notes)
                  .HasMaxLength(1000);

            entity.Property(e => e.Comment)
                  .HasMaxLength(1000);

            entity.Property(e => e.Filename)
                  .HasMaxLength(200);
        }

        /// <summary>
        /// Configure PrintHistory entity
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        private void ConfigurePrintHistory(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<PrintHistory>();

            // Primary key
            entity.HasKey(e => e.Id);

            // Indexes for performance
            entity.HasIndex(e => e.PrintedDate)
                  .HasDatabaseName("IX_PrintHistory_PrintedDate");

            entity.HasIndex(e => e.BarcodeRecordId)
                  .HasDatabaseName("IX_PrintHistory_BarcodeRecordId");

            entity.HasIndex(e => e.Success)
                  .HasDatabaseName("IX_PrintHistory_Success");

            // Foreign key relationship
            entity.HasOne(e => e.BarcodeRecord)
                  .WithMany(e => e.PrintHistories)
                  .HasForeignKey(e => e.BarcodeRecordId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Property configurations
            entity.Property(e => e.BarcodeRecordId)
                  .IsRequired();

            entity.Property(e => e.PrintedDate)
                  .IsRequired()
                  .HasDefaultValueSql("datetime('now')");

            entity.Property(e => e.QuantityPrinted)
                  .IsRequired();

            entity.Property(e => e.PrinterName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.LabelWidth)
                  .IsRequired();

            entity.Property(e => e.LabelHeight)
                  .IsRequired();

            entity.Property(e => e.BarcodeWidth)
                  .IsRequired();

            entity.Property(e => e.BarcodeHeight)
                  .IsRequired();

            entity.Property(e => e.Success)
                  .IsRequired()
                  .HasDefaultValue(true);

            entity.Property(e => e.ErrorMessage)
                  .HasMaxLength(1000);

            entity.Property(e => e.UserName)
                  .HasMaxLength(100);
        }

        /// <summary>
        /// Configure LabelTemplate entity
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        private void ConfigureLabelTemplate(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<LabelTemplate>();

            // Primary key
            entity.HasKey(e => e.Id);

            // Indexes for performance
            entity.HasIndex(e => e.IsDefault)
                  .HasDatabaseName("IX_LabelTemplates_IsDefault");

            entity.HasIndex(e => e.TemplateName)
                  .HasDatabaseName("IX_LabelTemplates_TemplateName");

            // Property configurations
            entity.Property(e => e.TemplateName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.LabelWidth)
                  .IsRequired();

            entity.Property(e => e.LabelHeight)
                  .IsRequired();

            entity.Property(e => e.BarcodeWidth)
                  .IsRequired();

            entity.Property(e => e.BarcodeHeight)
                  .IsRequired();

            entity.Property(e => e.IsDefault)
                  .IsRequired()
                  .HasDefaultValue(false);

            entity.Property(e => e.CreatedDate)
                  .IsRequired()
                  .HasDefaultValueSql("datetime('now')");
        }

        /// <summary>
        /// Seed initial data
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed default LabelTemplate
            modelBuilder.Entity<LabelTemplate>().HasData(
                new LabelTemplate
                {
                    Id = 1,
                    TemplateName = "Standard 100x50mm",
                    LabelWidth = 100.0,
                    LabelHeight = 50.0,
                    BarcodeWidth = 80.0,
                    BarcodeHeight = 20.0,
                    IsDefault = true,
                    CreatedDate = new DateTime(2025, 1, 1)
                }
            );

            // Seed additional common templates
            modelBuilder.Entity<LabelTemplate>().HasData(
                new LabelTemplate
                {
                    Id = 2,
                    TemplateName = "Small 75x25mm",
                    LabelWidth = 75.0,
                    LabelHeight = 25.0,
                    BarcodeWidth = 60.0,
                    BarcodeHeight = 15.0,
                    IsDefault = false,
                    CreatedDate = new DateTime(2025, 1, 1)
                },
                new LabelTemplate
                {
                    Id = 3,
                    TemplateName = "Large 150x75mm", 
                    LabelWidth = 150.0,
                    LabelHeight = 75.0,
                    BarcodeWidth = 120.0,
                    BarcodeHeight = 30.0,
                    IsDefault = false,
                    CreatedDate = new DateTime(2025, 1, 1)
                }
            );
        }

        /// <summary>
        /// Ensures the database is created with proper schema
        /// </summary>
        /// <returns>True if database was created, false if it already existed</returns>
        public bool EnsureCreated()
        {
            return Database.EnsureCreated();
        }

        /// <summary>
        /// Applies any pending migrations to the database
        /// </summary>
        public void Migrate()
        {
            Database.Migrate();
        }

        /// <summary>
        /// Gets the database connection string
        /// </summary>
        /// <returns>Connection string</returns>
        public string GetConnectionString()
        {
            return $"Data Source={DbPath}";
        }

        /// <summary>
        /// Checks if the database file exists
        /// </summary>
        /// <returns>True if database file exists</returns>
        public bool DatabaseExists()
        {
            return File.Exists(DbPath);
        }

        /// <summary>
        /// Gets database file size in bytes
        /// </summary>
        /// <returns>File size in bytes, -1 if file doesn't exist</returns>
        public long GetDatabaseSize()
        {
            if (File.Exists(DbPath))
            {
                var fileInfo = new FileInfo(DbPath);
                return fileInfo.Length;
            }
            return -1;
        }

        /// <summary>
        /// Backs up the database to a specified location
        /// </summary>
        /// <param name="backupPath">Backup file path</param>
        /// <returns>True if backup successful</returns>
        public bool BackupDatabase(string backupPath)
        {
            try
            {
                if (File.Exists(DbPath))
                {
                    File.Copy(DbPath, backupPath, true);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}