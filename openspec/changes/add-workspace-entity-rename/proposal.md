# Add Workspace Entity Rename

## Why

Users need the ability to rename portfolios and bots in the Blazor UI. Currently, the Key property is marked as `Editable="false"` in the UI to prevent crashes, which means users cannot rename existing entities. Since entity names are used as file/directory names in the workspace persistence layer, renaming requires coordinated updates across the UI, ViewModel, and data persistence layers.

This is a common operation needed for organizing and maintaining trading workspaces as users refine their bot names and portfolio organization over time.

## What Changes

- Add rename operation to the reactive persistence layer (`IObservableReaderWriter<TKey, TValue>`)
- Add rename command/action to the ViewModel layer (KeyValueVM or mixin)
- Add rename UI controls to ObservableDataView component (context menu or inline edit)
- Support both flat file storage (BotEntity) and directory-based storage (Portfolio2)
- Implement validation for duplicate names and invalid characters
- Maintain reactive cache updates and filesystem synchronization
- Make the solution generic and reusable for any workspace entity type

## Impact

**Affected specs:**
- `data-persistence` (new) - Reactive persistence layer with rename capability
- `ui-components` (new) - ObservableDataView rename controls
- `viewmodel` (new) - ViewModel rename command pattern

**Affected code:**
- Core repository persistence interfaces (IObservableReaderWriter, HjsonFsDirectoryReaderRx/WriterRx)
- Trading.Automation: KeyValueVM, Portfolio2VM, BotVM
- Trading.Automation.Blazor: ObservableDataView component
- Trading.Automation.Blazor: Portfolios.razor, Bots.razor pages

**Breaking changes:**
None - This is additive functionality

**Dependencies:**
- Requires understanding of VosFlags.PreferDirectory attribute to determine storage mode
- Requires FileSystemWatcher coordination to avoid conflicts
