# 🤝 Contributing to WPF Barcode Generator

Thank you for your interest in contributing! This document provides guidelines for contributing to the WPF Barcode Generator project.

## 📋 Before You Start

1. **Read the Documentation**
   - [README.md](README.md) - Project overview and features
   - [DEVELOPMENT.md](DEVELOPMENT.md) - Detailed development status and tasks
   - Check existing [Issues](../../issues) and [Pull Requests](../../pulls)

2. **Understand the Project Structure**
   - This is a **WPF .NET 8** application using **MVVM pattern**
   - **Entity Framework Core** with **SQLite** for data persistence
   - **ZXing.Net** for barcode generation  
   - **CommunityToolkit.Mvvm** for MVVM infrastructure

3. **Check Current Status**
   - **Phase 1**: ✅ Completed (Project setup, database, basic UI)
   - **Phase 2**: 🔄 In Progress (History window, messaging)
   - **Phase 3**: ⏳ Pending (Printing implementation)

## 🚀 Getting Started

### Development Environment
- **Visual Studio 2022** (recommended) or **VS Code** with C# extension
- **.NET 8 SDK** or later
- **Git** for version control

### Setup Instructions
```bash
# 1. Fork the repository on GitHub

# 2. Clone your fork
git clone https://github.com/YOUR_USERNAME/BarcodeGenerator.git
cd BarcodeGenerator

# 3. Add upstream remote
git remote add upstream https://github.com/ORIGINAL_OWNER/BarcodeGenerator.git

# 4. Create development branch
git checkout -b feature/your-feature-name

# 5. Restore packages and build
dotnet restore
dotnet build

# 6. Apply database migrations  
cd BarcodeGenerator
dotnet ef database update

# 7. Run the application
dotnet run
```

## 📝 Contribution Process

### 1. Choose Your Task

#### High Priority Tasks (Phase 2 Completion)
- 🔄 **Fix MainViewModel Message Handling** - Resolve compilation errors
- 🔄 **Complete History Window Testing** - Validate search, load, delete operations
- 🔄 **Implement Live Preview** - Real-time barcode preview with debouncing
- 🔄 **Phase 2 Testing & Validation** - Comprehensive testing of all features

#### Future Tasks (Phase 3)
- ⏳ **ZPL Generation** - Generate ZPL commands for Zebra printers
- ⏳ **Printer Service** - P/Invoke integration for raw printer communication
- ⏳ **Print Integration** - End-to-end printing workflow

#### Enhancement Opportunities
- 🎨 **UI/UX Improvements** - Better styling, animations, user experience
- ⚡ **Performance Optimization** - Database queries, UI responsiveness
- 🧪 **Testing Infrastructure** - Unit tests, integration tests
- 📚 **Documentation** - Code comments, user guides, API documentation

### 2. Create an Issue (Optional but Recommended)
- Describe the task or bug you want to work on
- Reference existing issues if related
- Wait for approval before starting large changes

### 3. Development Guidelines

#### Code Standards
```csharp
// Use MVVM pattern strictly
public partial class MainViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    
    [ObservableProperty]
    private string _barcodeText = string.Empty;
    
    [RelayCommand]
    private async Task SaveBarcodeAsync()
    {
        // Implementation
    }
}

// Always use async/await for database operations
public async Task<BarcodeRecord> SaveBarcodeRecordAsync(string text, string description)
{
    try
    {
        // Database operations
        await _context.SaveChangesAsync();
        return record;
    }
    catch (Exception ex)
    {
        // Handle errors gracefully
        throw new InvalidOperationException($"Failed to save barcode: {ex.Message}");
    }
}

// Proper disposal of resources
public void Dispose()
{
    _timer?.Stop();
    WeakReferenceMessenger.Default.UnregisterAll(this);
    _context?.Dispose();
}
```

#### Database Patterns
- **Always use async methods** for database operations
- **Soft delete** instead of hard delete (set `IsActive = false`)
- **Include navigation properties** when needed (`Include()` method)
- **Handle concurrency** gracefully
- **Add proper indexes** for search performance

#### UI Guidelines  
- **No code-behind logic** - Use data binding and commands
- **Responsive layouts** - Use Grid with proper column definitions
- **Loading indicators** - Show progress for long operations
- **Error handling** - Display user-friendly messages
- **Accessibility** - Proper tab order, tooltips, keyboard navigation

### 4. Testing Requirements

#### Before Submitting PR
- [ ] **Build succeeds** without warnings or errors
- [ ] **Application runs** and basic functionality works
- [ ] **Database operations** work (create, read, update, delete)
- [ ] **UI is responsive** and displays correctly
- [ ] **No memory leaks** (dispose resources properly)
- [ ] **Error handling** works for invalid inputs

#### Testing Checklist
```csharp
// Manual Testing
✅ Create barcode with valid input
✅ Save to database and verify persistence  
✅ Load barcode from history
✅ Search functionality in history window
✅ Delete barcode (soft delete)
✅ Preview updates correctly
✅ Handle invalid input gracefully
✅ Application startup and shutdown
```

### 5. Documentation Requirements
- **Code Comments**: Document complex logic and business rules
- **XML Documentation**: Public methods and properties
- **README Updates**: If adding new features or changing setup
- **DEVELOPMENT.md**: Update task status and technical details

## 🔧 Specific Contribution Areas

### Working on History Window (Current Priority)
```csharp
// Key files to understand:
- ViewModels/HistoryViewModel.cs    // Search, filtering, commands
- Views/HistoryWindow.xaml          // DataGrid, search UI
- ViewModels/MainViewModel.cs       // Message handling (needs fixing)

// Common tasks:
1. Fix BarcodeRecord property naming issues
2. Test message communication between windows
3. Validate search performance with large datasets  
4. Improve error handling and user feedback
```

### Working on Live Preview
```csharp
// Key files:
- ViewModels/MainViewModel.cs       // TriggerPreviewUpdate() method
- Services/BarcodeGeneratorService.cs // GeneratePreviewImage() 
- Views/MainWindow.xaml             // Preview panel binding

// Implementation approach:
1. Add DispatcherTimer with 300ms delay
2. Subscribe to property changes 
3. Generate composite image (barcode + text)
4. Convert to BitmapSource for WPF display
5. Handle errors gracefully (show placeholder)
```

### Working on Printing (Future)
```csharp
// Key areas:
- Helpers/ZplCommandGenerator.cs    // Generate ZPL commands
- Helpers/RawPrinterHelper.cs       // P/Invoke printer communication  
- Services/PrinterService.cs        // High-level printing logic

// Requirements:
1. Generate ZPL commands for Code 128 barcodes
2. Calculate positions and sizes (mm to dots conversion)
3. Send raw data to printer using Windows API
4. Handle multiple copies and print queuing
5. Log all print operations to database
```

## 📋 Pull Request Guidelines

### Before Creating PR
1. **Rebase on latest main**
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Test thoroughly**
   - Run full application
   - Test your specific changes
   - Check for regressions

3. **Update documentation**
   - Code comments
   - README if needed
   - DEVELOPMENT.md task status

### PR Title and Description
```
Title: [Area] Brief description of change

Description:
## What Changed
- Clear description of what was implemented/fixed

## Testing
- List of testing performed
- Any known limitations

## Related Issues  
- Closes #123
- Addresses #456

## Screenshots (if UI changes)
- Before/after images
```

### Code Review Process
1. **Automated checks** must pass (build, basic validation)
2. **Code review** by maintainer or experienced contributor
3. **Testing verification** - reviewer tests the changes
4. **Documentation review** - ensure docs are updated
5. **Merge approval** - changes merged to main branch

## 🐛 Bug Reports

### Creating Good Bug Reports
```
Title: [Component] Brief description of bug

## Description
Clear description of the problem

## Steps to Reproduce
1. Step one
2. Step two  
3. Step three

## Expected Behavior
What should happen

## Actual Behavior  
What actually happens

## Environment
- OS: Windows 10/11
- .NET Version: 8.0
- Visual Studio Version: 2022

## Additional Information
- Error messages
- Screenshots  
- Database state
```

## 💡 Feature Requests

### Proposing New Features
1. **Check existing issues** to avoid duplicates
2. **Describe the use case** - why is this needed?
3. **Propose solution** - how should it work?
4. **Consider alternatives** - other ways to solve the problem?
5. **Impact assessment** - breaking changes, complexity, etc.

## 🎯 Code Style and Conventions

### Naming Conventions
```csharp
// Classes: PascalCase
public class BarcodeGeneratorService { }

// Methods: PascalCase  
public async Task SaveBarcodeAsync() { }

// Properties: PascalCase
public string BarcodeText { get; set; }

// Fields: camelCase with underscore prefix
private readonly DatabaseService _databaseService;

// Constants: PascalCase
public const int MaxBarcodeLength = 35;

// Enums: PascalCase
public enum BarcodeType { Code128, QRCode }
```

### File Organization
```
// Group using statements
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using BarcodeGenerator.Models;
using BarcodeGenerator.Services;

// Namespace matches folder structure
namespace BarcodeGenerator.ViewModels
{
    // Class documentation
    /// <summary>
    /// Main view model for barcode generation and management
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        // Fields first
        private readonly DatabaseService _databaseService;
        
        // Properties
        [ObservableProperty]
        private string _barcodeText = string.Empty;
        
        // Commands
        [RelayCommand]
        private async Task SaveBarcodeAsync() { }
        
        // Methods
        public async Task InitializeAsync() { }
    }
}
```

## 🤝 Community Guidelines

### Be Respectful
- **Constructive feedback** - focus on code, not person
- **Patient with questions** - everyone is learning
- **Inclusive language** - welcoming to all backgrounds
- **Professional communication** - appropriate for workplace

### Collaboration  
- **Share knowledge** - explain your decisions
- **Ask questions** - when something is unclear
- **Review code thoughtfully** - help improve quality
- **Celebrate successes** - acknowledge good contributions

## 📞 Getting Help

### Where to Ask Questions
- **GitHub Issues** - For bugs and feature requests
- **GitHub Discussions** - For questions and general discussion
- **Code Comments** - For specific implementation questions
- **Pull Request Reviews** - For feedback on your changes

### Response Time Expectations
- **Issues**: Response within 1-2 business days
- **Pull Requests**: Initial review within 2-3 business days  
- **Questions**: Best effort, usually within 24 hours

---

**Thank you for contributing to the WPF Barcode Generator! Every contribution helps make this project better for everyone.** 🎉