# 🛠️ Development Guide - WPF Barcode Generator

This document provides detailed information for developers working on the WPF Barcode Generator project.

## 📊 Current Status (Updated: October 13, 2025)

### 🎯 Phase Overview
- ✅ **PHASE 1: COMPLETED** - Project Setup, Database, and Barcode Generation
- 🔄 **PHASE 2: IN PROGRESS** - History Window & Full Database CRUD  
- ⏳ **PHASE 3: PENDING** - Printing Implementation

### 🚀 Immediate Next Tasks
1. **Fix MainViewModel message handling compilation errors**
2. **Complete History Window testing and integration**
3. **Implement live preview functionality** 
4. **Phase 2 testing and validation**

---

## ✅ PHASE 1: COMPLETED TASKS

### 1. ✅ Project Setup & Database Core
**Status**: COMPLETED  
**Details**: 
- Created WPF .NET 8 project targeting .NET 8.0
- Installed EF Core packages (SQLite, Design, Tools)
- **Updated**: Switched from BarcodeLib to ZXing.Net for better .NET 8 compatibility
- Created proper folder structure: Models/, Data/, Services/, Helpers/, ViewModels/, Views/, Converters/

### 2. ✅ Database Entities & Context  
**Status**: COMPLETED  
**Details**:
- **BarcodeRecord**: 20+ properties including barcode data, settings, usage tracking
- **PrintHistory**: Complete audit trail with print details and success/failure tracking
- **LabelTemplate**: Reusable label size configurations  
- **AppDbContext**: Configured with indexes, relationships, and seeding
- All entities inherit from ObservableValidator for UI binding + validation

### 3. ✅ DatabaseService Implementation
**Status**: COMPLETED  
**Details**: Created 22 methods covering:
- **CRUD Operations**: Save, Get, Search, Delete for BarcodeRecord
- **Print History**: Logging, retrieval, statistics
- **Templates**: Management of label templates
- **Statistics**: Usage tracking and reporting
- **Search**: Advanced filtering and sorting capabilities

### 4. ✅ BarcodeGeneratorService with ZXing.Net
**Status**: COMPLETED  
**Details**:
- **Replaced BarcodeLib** with ZXing.Net for better .NET 8 support
- Implemented Code 128 barcode generation
- Added preview image generation with composite layout
- Proper System.Drawing to WPF BitmapSource conversion
- Input validation for Code 128 compatibility

### 5. ✅ Build Fixes & Migrations
**Status**: COMPLETED  
**Details**:
- Fixed all compilation errors (ObservableValidator inheritance, nullable references)
- Resolved XAML binding issues and converter problems  
- Database migrations created and applied successfully
- Build succeeds with only minor async method warnings (non-blocking)

### 6. ✅ MainViewModel Database Integration
**Status**: COMPLETED  
**Details**:
- Added DatabaseService injection in constructor
- Implemented SaveBarcodeCommand with full validation
- Added LoadBarcodeCommand with all settings restoration
- Created RecentBarcodes ObservableCollection for sidebar
- Added CurrentBarcodeRecord property for tracking loaded barcodes
- Integrated print logging with database operations

### 7. ✅ MainWindow 4-Column Layout
**Status**: COMPLETED  
**Details**: 
- **Column 0**: Settings Panel (350px) - Input controls, save button
- **Column 1**: Preview Panel (*) - Live barcode preview (needs completion)
- **Column 2**: Print Controls Panel (300px) - Printer selection, print buttons  
- **Column 3**: Recent Barcodes Sidebar (250px) - History list, load/reprint buttons
- Added Save button with proper command binding
- Implemented Recent barcodes list with action buttons

---

## 🔄 PHASE 2: IN PROGRESS TASKS

### 8. 🔄 History Window & ViewModel
**Status**: IN PROGRESS  
**Progress**: 85% Complete  
**Completed**:
- ✅ Created HistoryWindow.xaml with comprehensive DataGrid
- ✅ Implemented HistoryViewModel with search functionality  
- ✅ Added LoadSelectedCommand for barcode loading
- ✅ Created search, filter, and delete operations
- ✅ Added messaging between ViewModels using WeakReferenceMessenger

**Remaining Work**:
- 🔄 Fix compilation errors in MainViewModel message handling
- 🔄 Test message communication between History and Main windows
- 🔄 Validate search functionality with real data
- 🔄 Test delete operations and UI updates

**Technical Details**:
```csharp
// Message types created:
public record LoadBarcodeMessage(BarcodeRecord Barcode);
public record ReprintBarcodeMessage(BarcodeRecord Barcode);

// HistoryViewModel features:
- Search with real-time filtering
- DataGrid with sortable columns  
- Action buttons (Load, Reprint, Delete)
- Statistics display
- Loading indicators

// MainViewModel integration:
- Message handlers registered in constructor
- OnLoadBarcodeMessage() - loads barcode settings
- OnReprintBarcodeMessage() - triggers reprint workflow
```

**Known Issues**:
- Compilation errors in MainViewModel message handlers
- BarcodeRecord property naming mismatches need fixing
- Message unregistration needed in Dispose()

---

## ⏳ PHASE 3: PENDING TASKS

### 9. ⏳ Live Preview Implementation  
**Status**: NOT STARTED  
**Priority**: HIGH (needed for Phase 2 completion)  
**Requirements**:
- Update MainViewModel to generate real-time preview using BarcodeGeneratorService
- Implement debouncing (300ms timer) to prevent excessive updates
- Update PreviewPanel in MainWindow to show live barcode preview as user types
- Handle errors gracefully (show placeholder for invalid input)

**Technical Approach**:
```csharp
// In MainViewModel:
private DispatcherTimer _previewUpdateTimer;

// Property change handlers trigger:
private void TriggerPreviewUpdate()
{
    _previewUpdateTimer.Stop();
    _previewUpdateTimer.Start(); // 300ms delay
}

// Timer tick generates preview:
private async void OnPreviewUpdateTimerTick(object sender, EventArgs e)
{
    _previewUpdateTimer.Stop();
    await UpdatePreviewAsync();
}
```

### 10. ⏳ Testing & Validation
**Status**: NOT STARTED  
**Priority**: HIGH  
**Scope**: Comprehensive testing of all Phase 1 and 2 functionality
**Requirements**:
- Test barcode generation with various inputs
- Validate database operations (save, load, search, delete)
- Test UI responsiveness and error handling  
- Verify data persistence across application restarts
- Validate search functionality performance
- Test messaging between ViewModels

---

## 🖨️ PHASE 3: PRINTING IMPLEMENTATION (Future)

### ZPL Generation
- Create ZplCommandGenerator helper class
- Implement ConvertMmToDots method for printer calculations
- Generate complete ZPL command strings for Code 128
- Handle multiple copies and positioning

### Printer Service  
- Implement GetAvailablePrinters() method
- Create RawPrinterHelper using P/Invoke for direct printer communication
- Add SendZplToPrinter method for Zebra printers
- Handle printer status checking and error reporting

### Print Integration
- Update PrintCommand in MainViewModel for actual printing
- Integrate DatabaseService.LogPrintHistoryAsync for audit trail
- Update TotalPrintCount and LastPrintedDate tracking
- Comprehensive error handling and user feedback

---

## 🛠️ Development Guidelines

### Code Standards
- **MVVM Pattern**: Strict separation of concerns
- **Async/Await**: Use for all database and long-running operations  
- **ObservableObject**: Inherit from CommunityToolkit base classes
- **Dependency Injection**: Constructor injection for services
- **Error Handling**: Try-catch with user-friendly messages
- **Dispose Pattern**: Proper cleanup of resources and message handlers

### Database Patterns
- **Entity Validation**: Use DataAnnotations and ObservableValidator
- **Soft Delete**: Mark records inactive instead of physical deletion
- **Audit Trails**: Track all modifications with timestamps and user info
- **Optimistic Updates**: UI updates immediately, sync with database async

### UI Guidelines  
- **Data Binding**: No code-behind logic, pure XAML binding
- **Commands**: Use RelayCommand with CanExecute validation
- **Loading States**: Show progress indicators for async operations
- **Error Display**: StatusMessage property for user feedback
- **Responsive Layout**: Grid columns with proper sizing (*, Auto, fixed)

### Testing Strategy
- **Manual Testing**: Use comprehensive checklist for each feature
- **Database Testing**: Test with empty, small, and large datasets
- **Error Testing**: Invalid inputs, database failures, printer issues
- **Integration Testing**: Full workflows from UI through database
- **Performance Testing**: Large datasets, rapid input changes

---

## 📝 Current Issues & Solutions

### Issue 1: MainViewModel Message Handling Compilation Errors
**Problem**: LoadBarcodeMessage and ReprintBarcodeMessage not recognized  
**Solution**: Import correct message types, verify property names match BarcodeRecord entity  
**Status**: 🔄 In Progress

### Issue 2: Live Preview Not Implemented  
**Problem**: Preview panel shows placeholder, no real-time updates  
**Solution**: Implement TriggerPreviewUpdate() method with debouncing timer  
**Status**: ⏳ Pending

### Issue 3: BarcodeRecord Property Name Mismatches
**Problem**: HistoryViewModel references non-existent properties  
**Solution**: Update to use correct property names from BarcodeRecord entity  
**Status**: 🔄 In Progress

---

## 🚀 Getting Started for New Developers

### 1. Environment Setup
```bash
# Clone repository
git clone [repository-url]
cd BarcodeGenerator

# Restore packages  
dotnet restore

# Apply migrations
cd BarcodeGenerator
dotnet ef database update

# Build and run
dotnet build
dotnet run
```

### 2. Understanding the Architecture
- **Study MainViewModel.cs**: Central hub with database integration
- **Review DatabaseService.cs**: All 22 CRUD operations
- **Examine BarcodeRecord.cs**: Dual-purpose entity + UI model
- **Check AppDbContext.cs**: EF Core configuration and relationships

### 3. Current Development Focus
- Pick up History Window completion (message handling fixes)
- Implement live preview functionality  
- Add comprehensive testing for Phase 2 features
- Prepare for Phase 3 printing implementation

### 4. Testing Your Changes
- Run application and test basic barcode creation
- Test database persistence (close/reopen app)  
- Test History window (search, load, delete operations)
- Verify no compilation warnings or errors

---

## 📋 Task Assignment

**Available for Assignment**:
- 🔄 **Fix MainViewModel Message Handling** (HIGH PRIORITY)
- 🔄 **Complete History Window Testing** (HIGH PRIORITY)  
- ⏳ **Implement Live Preview** (MEDIUM PRIORITY)
- ⏳ **Comprehensive Testing & Validation** (MEDIUM PRIORITY)

**Future Tasks**:
- ⏳ **ZPL Generation Implementation**
- ⏳ **Printer Service Development**  
- ⏳ **End-to-End Print Workflow**

---

**For questions or task assignment, please create an issue or contact the project maintainer.**