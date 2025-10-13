# 🏷️ WPF Barcode Generator & Printer

A comprehensive WPF .NET 8 application for generating Code 128 barcodes with database persistence, history management, and Zebra printer integration.

## 📊 Project Status (Updated: October 13, 2025)

### ✅ **PHASE 1: COMPLETED** - Project Setup, Database, and Barcode Generation
- ✅ **Project Setup & Database Core** - WPF .NET 8, EF Core SQLite, ZXing.Net
- ✅ **Database Entities & Context** - BarcodeRecord, PrintHistory, LabelTemplate entities
- ✅ **DatabaseService Implementation** - 22 CRUD methods for complete data management
- ✅ **BarcodeGeneratorService** - ZXing.Net integration for Code 128 generation
- ✅ **Build Fixes & Migrations** - All compilation errors resolved
- ✅ **MainViewModel Database Integration** - Save/Load/Reprint functionality
- ✅ **MainWindow 4-Column Layout** - Settings, Preview, Print Controls, History Sidebar

### 🔄 **PHASE 2: IN PROGRESS** - History Window & Full Database CRUD
- ✅ **History Window Created** - DataGrid with search, action buttons
- ✅ **HistoryViewModel** - Search, load, delete operations
- ✅ **Messaging Between ViewModels** - WeakReferenceMessenger communication
- 🔄 **Testing & Integration** - Needs completion and validation

### ⏳ **PHASE 3: PENDING** - Printing Implementation
- ⏳ ZPL Generation for Zebra printers
- ⏳ Raw printer P/Invoke implementation
- ⏳ End-to-end print workflow

## 🚀 Features

### Core Functionality
- **Barcode Generation**: Code 128 barcodes using ZXing.Net
- **Database Persistence**: SQLite database with EF Core
- **History Management**: Full CRUD operations with search
- **Preview System**: Real-time barcode preview
- **Settings Persistence**: Remember label sizes and quantities
- **4-Column Layout**: Organized UI for optimal workflow

### Database Features
- **BarcodeRecord**: Store barcode text, description, settings, usage statistics
- **PrintHistory**: Complete audit trail of all print jobs
- **LabelTemplate**: Reusable label size configurations
- **Statistics**: Track usage patterns and print counts

### UI Components
- **Settings Panel**: Barcode input, label dimensions, quantity controls
- **Preview Panel**: Live barcode preview with label simulation
- **Print Controls**: Printer selection and print management
- **History Sidebar**: Recent barcodes with one-click reload/reprint
- **History Window**: Full database view with search and management

## 🛠️ Technology Stack

- **Framework**: WPF .NET 8
- **Database**: Entity Framework Core 9.0.9 with SQLite
- **MVVM**: CommunityToolkit.Mvvm 8.4.0
- **Barcode**: ZXing.Net (replaced BarcodeLib for better .NET 8 compatibility)
- **Messaging**: WeakReferenceMessenger for ViewModel communication
- **UI**: Material-inspired styling with custom controls

## 📁 Project Structure

```
BarcodeGenerator/
├── 📁 Data/                    # Database context and entities
│   ├── AppDbContext.cs         # EF Core database context
│   └── Entities/               # Database entity models
├── 📁 Models/                  # Application models
│   ├── BarcodeData.cs          # Barcode generation model
│   ├── BarcodeRecord.cs        # Database entity + UI model
│   ├── LabelSettings.cs        # Label configuration model
│   └── PrintHistory.cs         # Print history entity
├── 📁 Services/                # Business logic services
│   ├── BarcodeGeneratorService.cs  # ZXing.Net barcode generation
│   ├── DatabaseService.cs      # Database CRUD operations (22 methods)
│   ├── PrinterService.cs       # Printer management (future)
│   └── SettingsService.cs      # Application settings
├── 📁 ViewModels/              # MVVM ViewModels
│   ├── MainViewModel.cs        # Main window logic + database integration
│   └── HistoryViewModel.cs     # History window logic + search
├── 📁 Views/                   # WPF Windows and UserControls
│   ├── MainWindow.xaml         # 4-column main interface
│   └── HistoryWindow.xaml      # History management window
├── 📁 Helpers/                 # Utility classes
│   ├── ImageHelper.cs          # Image conversion utilities
│   └── ValidationHelper.cs     # Input validation
├── 📁 Converters/              # WPF value converters
├── 📁 Resources/               # Images, icons, styles
└── 📁 Migrations/              # EF Core database migrations
```

## 🚀 Getting Started

### Prerequisites
- **Visual Studio 2022** or **VS Code** with C# extension
- **.NET 8 SDK** or later
- **Git** for version control

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/[USERNAME]/BarcodeGenerator.git
   cd BarcodeGenerator
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run database migrations**
   ```bash
   cd BarcodeGenerator
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

### First Time Setup
- The SQLite database will be created automatically in `%LocalAppData%/BarcodeLabelerPrinter/`
- A default label template (100x50mm) will be seeded
- The application is ready to use immediately

## 📖 Usage Guide

### Creating Barcodes
1. **Enter barcode text** (max 35 characters for Code 128)
2. **Add description** for easy identification
3. **Set label dimensions** (width x height in mm)
4. **Set barcode size** within the label
5. **Choose quantity** for printing
6. **Click "Save Barcode"** to store in database

### Managing History
- **Recent Barcodes Sidebar**: Shows last used barcodes
- **Load**: One-click to reload all settings
- **Reprint**: Use saved quantity and settings
- **View Full History**: Opens comprehensive management window
- **Search**: Find barcodes by text or description
- **Delete**: Remove unwanted entries

### Database Features
- **Automatic saving**: Settings remembered for each barcode
- **Usage tracking**: Print counts and last used dates
- **Soft delete**: Barcodes marked inactive, not permanently deleted
- **Audit trail**: Complete history of all operations

## 🏗️ Architecture

### MVVM Pattern
- **Models**: Data structures and business entities
- **ViewModels**: UI logic and data binding (ObservableObject base)
- **Views**: XAML UI with no code-behind logic
- **Services**: Business logic and data access

### Database Design
- **BarcodeRecord**: Core entity with 20+ properties
- **PrintHistory**: Audit trail for all print operations
- **LabelTemplate**: Reusable size configurations
- **Indexes**: Optimized for search and filtering

### Messaging System
- **WeakReferenceMessenger**: Loose coupling between ViewModels
- **LoadBarcodeMessage**: History → Main communication
- **ReprintBarcodeMessage**: Direct reprint requests

## 🛠️ Development Tasks

### Current Status
See [DEVELOPMENT.md](DEVELOPMENT.md) for detailed task breakdown and progress tracking.

### Next Priorities
1. ✅ Complete History Window testing
2. ✅ Fix MainViewModel message handling
3. 🔄 Implement live preview functionality  
4. 🔄 Complete Phase 2 validation
5. ⏳ Begin Phase 3: Printing implementation

### Contributing
1. Check [DEVELOPMENT.md](DEVELOPMENT.md) for current task status
2. Pick an unassigned task or create an issue
3. Follow the existing code patterns and MVVM architecture
4. Test thoroughly before submitting PR
5. Update documentation for new features

## 🧪 Testing

### Manual Testing Checklist
- [ ] Barcode generation with various inputs
- [ ] Database save/load operations
- [ ] History window search functionality
- [ ] Settings persistence across sessions
- [ ] Preview updates in real-time
- [ ] Error handling for invalid inputs

### Automated Testing (Future)
- Unit tests for services and ViewModels
- Integration tests for database operations
- UI automation tests for critical workflows

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## 📞 Support

- **Issues**: Use GitHub Issues for bug reports and feature requests
- **Discussions**: Use GitHub Discussions for questions and ideas
- **Wiki**: Check the project Wiki for additional documentation

## 🔄 Version History

- **v0.2.0** - History Window & Database Integration (Current)
- **v0.1.0** - Initial barcode generation and 4-column layout
- **v0.0.1** - Project setup and basic structure

---

**Made with ❤️ for efficient barcode management and printing workflows**