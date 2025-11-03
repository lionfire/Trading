# Implementation Tasks

## 1. Data Layer - Core Persistence

### 1.1 Define RenameResult Type
- [ ] Create `RenameResult<TKey>` record in `/src/Core` persistence namespace
- [ ] Add `Success`, `NewKey`, `Error`, and `ErrorCode` properties
- [ ] Add convenience factory methods: `Success(TKey)`, `Failure(string, string?)`
- [ ] Add XML documentation comments

### 1.2 Extend IObservableReaderWriter Interface
- [ ] Add `RenameAsync(TKey oldKey, TKey newKey, CancellationToken)` method to interface
- [ ] Add XML documentation with example usage
- [ ] Consider backward compatibility - mark as optional implementation

### 1.3 Implement Storage Mode Detection Helper
- [ ] Create `VosAttributeHelper` class with `IsDirectoryMode<T>()` method
- [ ] Use reflection to check for `VosFlags.PreferDirectory` attribute
- [ ] Add caching for reflection results (use ConcurrentDictionary)
- [ ] Add unit tests for various entity types

### 1.4 Implement Rename in HjsonFsDirectoryWriterRx
- [ ] Locate `HjsonFsDirectoryWriterRx<TKey, TValue>` in `/src/Core`
- [ ] Implement `RenameAsync` method with flat file support
- [ ] Implement `RenameAsync` method with directory support
- [ ] Add filesystem validation (check for duplicates)
- [ ] Add filename validation (invalid characters)
- [ ] Use `File.Move` and `Directory.Move` for atomic operations
- [ ] Handle IOException and convert to RenameResult
- [ ] Add logging for rename operations

### 1.5 Update Observable Cache After Rename
- [ ] Remove old key from SourceCache
- [ ] Update entity Key property in memory (if mutable)
- [ ] Add entity with new key to SourceCache
- [ ] Trigger appropriate change notifications
- [ ] Consider FileSystemWatcher interaction (may need to suppress duplicate events)

### 1.6 Write Unit Tests for Data Layer
- [ ] Test rename flat file success
- [ ] Test rename directory success
- [ ] Test rename failure - duplicate name
- [ ] Test rename failure - invalid characters
- [ ] Test rename failure - file locked
- [ ] Test storage mode detection for both modes
- [ ] Test cache update consistency

## 2. ViewModel Layer

### 2.1 Create IRenameableVM Interface
- [ ] Define interface in `/src/Trading/Automation/Mvvm` namespace
- [ ] Add `RenameCommand` property of type `ICommand` or `ReactiveCommand`
- [ ] Add `ErrorMessage` property for binding
- [ ] Add XML documentation

### 2.2 Extend KeyValueVM with Rename Capability
- [ ] Add `IObservableReaderWriter<TKey, TValue>` dependency to constructor
- [ ] Implement `IRenameableVM<TKey>` interface
- [ ] Create `RenameCommand` as `ReactiveCommand<string, RenameResult<TKey>>`
- [ ] Add validation logic (empty, whitespace, invalid chars)
- [ ] Call persistence layer `RenameAsync` method
- [ ] Update Key property on success
- [ ] Handle errors and populate ErrorMessage property
- [ ] Add proper command CanExecute logic (disable during operation)

### 2.3 Create Validation Helpers
- [ ] Create `EntityNameValidator` static class
- [ ] Add `IsValidEntityName(string)` method
- [ ] Add `GetInvalidCharacters()` method
- [ ] Add `SanitizeName(string)` method (trim, remove invalid chars)
- [ ] Add unit tests for validation logic

### 2.4 Write Unit Tests for ViewModel Layer
- [ ] Test RenameCommand with valid name - success
- [ ] Test RenameCommand with empty name - validation failure
- [ ] Test RenameCommand with invalid characters - validation failure
- [ ] Test RenameCommand with duplicate name - error handling
- [ ] Test RenameCommand disabled during execution
- [ ] Test ErrorMessage populated on failure
- [ ] Test ErrorMessage cleared on new operation

## 3. UI Layer - Blazor Components

### 3.1 Locate or Create ObservableDataView Context Menu Section
- [ ] Find ObservableDataView component in `/src/Core` or Trading.Blazor
- [ ] Review existing ContextMenu implementation (if any)
- [ ] If no ContextMenu exists, add MudMenu to each row

### 3.2 Add Rename Menu Item
- [ ] Add MudMenuItem with "Rename" text and appropriate icon (e.g., Edit icon)
- [ ] Bind Click event to open rename dialog
- [ ] Pass current entity Key to dialog
- [ ] Check if ViewModel implements IRenameableVM to enable/disable menu item

### 3.3 Create RenameDialog Component
- [ ] Create `RenameDialog.razor` in `/src/Trading/Automation.Blazor/Dialogs`
- [ ] Add MudDialog with title "Rename [EntityType]"
- [ ] Add MudTextField bound to newName parameter
- [ ] Initialize with current name selected
- [ ] Add real-time validation (use MudTextField Validation)
- [ ] Add OK and Cancel buttons
- [ ] Add loading indicator (MudProgressCircular)
- [ ] Bind OK button to ViewModel RenameCommand

### 3.4 Implement Dialog Logic
- [ ] Create `RenameDialog.razor.cs` code-behind
- [ ] Add parameters: CurrentName, OnRename callback
- [ ] Add validation logic matching ViewModel
- [ ] Handle OK button click - invoke OnRename callback
- [ ] Handle Cancel button click - close dialog
- [ ] Show loading state during async operation
- [ ] Show error message on failure
- [ ] Close dialog and show success notification on success

### 3.5 Integrate with Portfolios and Bots Pages
- [ ] Update `/src/Trading/Automation.Blazor/Portfolios/Portfolios.razor`
- [ ] Add Rename menu item to existing ContextMenu (or create one)
- [ ] Inject IDialogService
- [ ] Create method to open RenameDialog
- [ ] Update `/src/Trading/Automation.Blazor/Bots/Bots.razor` similarly
- [ ] Test rename on both pages

### 3.6 Add Success/Error Notifications
- [ ] Use MudSnackbar for success notifications
- [ ] Show "Renamed to [NewName]" message
- [ ] Use appropriate severity colors (success green, error red)
- [ ] Auto-dismiss success notifications after 3 seconds

### 3.7 Keyboard Navigation and Accessibility
- [ ] Add keyboard shortcut handling (Enter = OK, Escape = Cancel)
- [ ] Set initial focus to text field
- [ ] Select current name text on dialog open
- [ ] Add ARIA labels to all dialog controls
- [ ] Add aria-live region for error messages
- [ ] Test with screen reader (NVDA or JAWS)

### 3.8 UI Testing
- [ ] Manual test rename portfolio (directory-based)
- [ ] Manual test rename bot (flat file)
- [ ] Test duplicate name error handling
- [ ] Test invalid character error handling
- [ ] Test cancel operation
- [ ] Test keyboard navigation
- [ ] Test loading state display
- [ ] Test success/error notifications
- [ ] Test with screen reader

## 4. Integration and Testing

### 4.1 Integration Tests
- [ ] Create integration test project or test class
- [ ] Test full rename flow: UI → ViewModel → Data Layer → Filesystem
- [ ] Test reactive cache updates propagate to UI
- [ ] Test FileSystemWatcher doesn't cause conflicts
- [ ] Test concurrent access scenarios

### 4.2 Error Scenario Testing
- [ ] Test rename while file is open in another application
- [ ] Test rename with filesystem permissions issues
- [ ] Test rename with very long names (filesystem limits)
- [ ] Test rename with Unicode characters
- [ ] Test rename on case-insensitive vs case-sensitive filesystems

### 4.3 Performance Testing
- [ ] Measure rename operation latency
- [ ] Test with large numbers of entities (1000+ portfolios)
- [ ] Ensure UI remains responsive during rename

### 4.4 Documentation
- [ ] Update project.md with rename capability description
- [ ] Add code comments to complex logic
- [ ] Create user documentation or help text for rename feature
- [ ] Update CLAUDE.md if needed

## 5. Deployment and Validation

### 5.1 Code Review Preparation
- [ ] Review all changed files for code quality
- [ ] Ensure consistent naming and style
- [ ] Check for proper error handling
- [ ] Verify XML documentation completeness

### 5.2 Pre-Deployment Checklist
- [ ] All unit tests passing
- [ ] All integration tests passing
- [ ] Manual testing complete for all scenarios
- [ ] No compiler warnings
- [ ] Code formatted and cleaned up

### 5.3 Deployment
- [ ] Merge changes to main branch
- [ ] Deploy to development environment
- [ ] Test in development environment
- [ ] Monitor logs for errors
- [ ] Deploy to production

### 5.4 Post-Deployment Validation
- [ ] Test rename in production environment
- [ ] Monitor for filesystem errors
- [ ] Check user feedback
- [ ] Address any issues promptly

## Dependencies and Sequencing

**Critical Path:**
1. Tasks 1.1-1.6 must complete before 2.x (data layer before viewmodel)
2. Tasks 2.1-2.4 must complete before 3.x (viewmodel before UI)
3. Tasks 3.1-3.5 must complete before 3.6-3.8 (basic UI before polish)

**Parallel Work:**
- Task 1.3 (storage mode detection) can run in parallel with 1.2 (interface)
- Task 2.3 (validation helpers) can run in parallel with 2.2 (KeyValueVM)
- Task 3.3 (dialog component) can start as soon as 2.2 is done (no need to wait for all ViewModel tests)

**External Dependencies:**
- None - all work is within existing project structure
