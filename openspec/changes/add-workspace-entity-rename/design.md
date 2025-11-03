# Workspace Entity Rename - Technical Design

## Context

The LionFire Trading platform uses a reactive MVVM architecture with file-based persistence. Entities like Portfolio2 and BotEntity are stored as HJSON files in workspace directories. The Key property (entity name) determines the filename or directory name:

- **Flat files**: `BotEntitys/MyBot.bot.hjson` (Key = "MyBot")
- **Directories**: `Portfolio2s/~New~XXX/portfolio.hjson` (Key = "~New~XXX")

Storage mode is determined by the `[Vos(VosFlags.PreferDirectory)]` attribute on the entity class.

Current constraint: The Key property is marked `Editable="false"` in ObservableDataView to prevent crashes because editing the Key doesn't rename the underlying file/directory, causing data loss.

## Goals / Non-Goals

**Goals:**
- Enable renaming portfolios and bots through the Blazor UI
- Support both flat file and directory-based storage patterns
- Maintain reactive cache consistency during rename
- Provide reusable rename capability for any workspace entity type
- Validate new names (duplicates, invalid characters)
- Handle concurrent access and filesystem errors gracefully

**Non-Goals:**
- Renaming entities across different workspaces
- Undo/redo functionality
- Batch rename operations
- Automatic name suggestions or AI-powered naming

## Decisions

### 1. Three-Layer Architecture

**Decision:** Implement rename at three layers (Data, ViewModel, UI) for proper separation of concerns.

**Rationale:**
- **Data Layer**: Core rename logic with filesystem operations
- **ViewModel Layer**: Command pattern for UI binding
- **UI Layer**: User interaction (context menu or inline edit)

This maintains the existing MVVM pattern and keeps each layer focused on its responsibility.

### 2. Add Rename to IObservableReaderWriter

**Decision:** Extend `IObservableReaderWriter<TKey, TValue>` with an optional rename method:

```csharp
Task<RenameResult<TKey>> RenameAsync(TKey oldKey, TKey newKey, CancellationToken cancellationToken = default);
```

**Alternatives considered:**
- Create separate `IRenameable<TKey>` interface - More flexible but adds complexity
- Put rename in ViewModel only - Doesn't address filesystem operations
- Use Delete + Add - Loses metadata and history

**Rationale:**
- Keeps related operations together
- Allows persistence layer to handle atomic filesystem operations
- Can validate at the data layer before making changes
- Returns structured result with success/error information

### 3. Storage Mode Detection

**Decision:** Use reflection to check for `VosFlags.PreferDirectory` attribute to determine whether to rename a file or directory.

```csharp
bool isDirectoryMode = typeof(TValue).GetCustomAttribute<VosAttribute>()?.Flags.HasFlag(VosFlags.PreferDirectory) ?? false;
```

**Rationale:**
- Already established pattern in the codebase
- Avoids hardcoding entity-specific logic
- Single source of truth for storage mode

### 4. Context Menu for Rename UI

**Decision:** Add "Rename" option to ObservableDataView context menu (existing ContextMenu section).

**Alternatives considered:**
- Make Key column editable - Confusing UX, requires Enter/Escape handling
- Add rename button to toolbar - Takes up space, less discoverable
- Double-click to rename - Conflicts with row expansion

**Rationale:**
- Consistent with standard UI patterns (Explorer, IDEs)
- Doesn't clutter the UI
- Easy to discover via right-click

### 5. MudBlazor Dialog for Rename Input

**Decision:** Use MudDialog with MudTextField for entering the new name.

**Rationale:**
- Consistent with MudBlazor design system
- Built-in validation support
- Clear focus on the rename operation
- Can show validation errors inline

### 6. Reactive Cache Update Strategy

**Decision:** Update the Key in the SourceCache after successful filesystem rename:

1. Perform filesystem rename
2. Update entity Key property in memory
3. Remove old key from cache
4. Add updated entity with new key to cache
5. Trigger change notification

**Rationale:**
- Maintains cache consistency
- FileSystemWatcher will also detect the change and can be ignored
- Avoids race conditions by updating cache immediately after filesystem operation

## Risks / Trade-offs

### Risk: Concurrent Rename Operations
**Mitigation:**
- Use file locking during rename
- Return clear error if file is locked
- Show error dialog to user

### Risk: FileSystemWatcher Conflicts
**Mitigation:**
- Temporarily disable watcher during rename OR
- Use rename transaction ID to ignore self-triggered events

### Risk: Failed Partial Rename
**Mitigation:**
- For directory renames, use atomic filesystem operations
- Return RenameResult with specific error information
- Don't update cache if filesystem operation fails

### Trade-off: Generic vs Specific
**Decision:** Implement generic solution even though only Portfolio2 and BotEntity need it initially.

**Rationale:**
- Prevents code duplication for future entity types
- Encourages consistent UX across all workspace entities
- Small additional complexity for significant maintainability benefit

## Implementation Plan

### Phase 1: Data Layer
1. Define `RenameResult<TKey>` class
2. Add `RenameAsync` to `IObservableReaderWriter<TKey, TValue>`
3. Implement in `HjsonFsDirectoryReaderRx<TKey, TValue>` and `HjsonFsDirectoryWriterRx<TKey, TValue>`
4. Add storage mode detection helper
5. Write unit tests for both storage modes

### Phase 2: ViewModel Layer
1. Add `IRenameableVM<TKey>` interface
2. Add `RenameCommand` to `KeyValueVM<TKey, TValue>`
3. Implement command logic with validation
4. Handle async operations and error states

### Phase 3: UI Layer
1. Add Rename menu item to ObservableDataView ContextMenu
2. Create rename dialog with MudDialog
3. Bind to ViewModel RenameCommand
4. Show success/error feedback
5. Test with Portfolios and Bots pages

### Phase 4: Testing & Polish
1. Integration tests for full rename flow
2. Error handling scenarios
3. Validation edge cases
4. UI polish and accessibility

## Migration Plan

No migration required - this is additive functionality. Existing entities continue to work as before.

**Deployment:**
1. Deploy data layer changes (backward compatible)
2. Deploy ViewModel changes (backward compatible)
3. Deploy UI changes (feature becomes available)
4. Monitor for filesystem errors and validation issues

**Rollback:**
- Remove UI changes (users lose rename button)
- Existing entities and operations unaffected

## Open Questions

1. **Should we keep a rename history/audit log?**
   - **Decision:** Not in MVP - can add later if needed

2. **Should we allow renaming of entities that are currently "in use" (e.g., running bots)?**
   - **Decision:** Allow rename but show warning if entity is active. Let the system handle any downstream updates through reactive notifications.

3. **Should we validate against filesystem naming restrictions (length, special characters)?**
   - **Decision:** Yes - validate against Windows and Linux filename restrictions, show clear error messages.

4. **Should rename be case-sensitive?**
   - **Decision:** Follow filesystem behavior (case-insensitive on Windows, case-sensitive on Linux). Treat case-only renames as valid.
