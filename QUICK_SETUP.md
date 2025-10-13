# 🚀 Quick Setup Guide

## For New Developers

### 1. Clone and Setup (5 minutes)
```bash
# Clone the repository
git clone https://github.com/[USERNAME]/BarcodeGenerator.git
cd BarcodeGenerator

# Restore packages
dotnet restore

# Apply database migrations
cd BarcodeGenerator
dotnet ef database update

# Build and run
dotnet build
dotnet run
```

### 2. What You'll See
- **4-column interface**: Settings | Preview | Print Controls | History
- **Database auto-created** in: `%LocalAppData%/BarcodeLabelerPrinter/`
- **Default template** seeded: 100x50mm label size

### 3. Current Status
- ✅ **Phase 1**: Completed (barcode generation, database, UI)
- 🔄 **Phase 2**: 85% done (history window needs testing)
- ⏳ **Phase 3**: Pending (printing implementation)

### 4. Next Tasks (Pick One)
1. **Fix MainViewModel Message Handling** (HIGH) - Compilation errors
2. **Complete History Window Testing** (HIGH) - Integration testing
3. **Implement Live Preview** (MED) - Real-time barcode display
4. **Add UI Polish** (LOW) - Styling improvements

### 5. Key Files to Understand
- `ViewModels/MainViewModel.cs` - Main application logic
- `Services/DatabaseService.cs` - 22 database methods
- `Views/MainWindow.xaml` - 4-column UI layout
- `Models/BarcodeRecord.cs` - Database entity + UI model

### 6. Getting Help
- **README.md** - Complete project overview
- **DEVELOPMENT.md** - Detailed task breakdown
- **CONTRIBUTING.md** - Development guidelines
- **GitHub Issues** - Ask questions or report bugs

## For Project Owner

### Sharing with Developers
1. **Create GitHub repository** with this code
2. **Share this URL** with collaborators
3. **Point them to DEVELOPMENT.md** for tasks
4. **Use GitHub Issues** for task assignment

### Repository Setup
```bash
# Create GitHub repository first, then:
git remote add origin https://github.com/[USERNAME]/BarcodeGenerator.git
git branch -M main
git push -u origin main
```

---
**Ready to code! Check DEVELOPMENT.md for available tasks.** 🎯