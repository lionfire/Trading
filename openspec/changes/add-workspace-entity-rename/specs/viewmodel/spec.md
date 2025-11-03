# ViewModel - Spec Delta

## ADDED Requirements

### Requirement: Renameable ViewModel Interface

ViewModels for workspace entities SHALL implement a rename command that can be bound to UI controls.

#### Scenario: ViewModel exposes rename command

- **GIVEN** a KeyValueVM instance wrapping an entity
- **WHEN** the UI queries for available commands
- **THEN** a RenameCommand is available
- **AND** the command can be executed with a new name parameter
- **AND** the command reports its CanExecute state

#### Scenario: Rename command validation

- **GIVEN** a KeyValueVM with current key "OldName"
- **WHEN** RenameCommand is invoked with an empty string
- **THEN** the command does not execute
- **AND** validation feedback is provided

#### Scenario: Rename command success updates ViewModel

- **GIVEN** a KeyValueVM with current key "OldName"
- **WHEN** RenameCommand executes successfully with "NewName"
- **THEN** the ViewModel's Key property is updated to "NewName"
- **AND** PropertyChanged events are raised for the Key property
- **AND** the underlying persistence layer is updated

#### Scenario: Rename command failure preserves state

- **GIVEN** a KeyValueVM with current key "OldName"
- **WHEN** RenameCommand executes but the rename fails
- **THEN** the ViewModel's Key property remains "OldName"
- **AND** an error message is available for display
- **AND** no PropertyChanged events are raised

### Requirement: Async Command Execution

The rename command SHALL support asynchronous execution to prevent UI blocking during filesystem operations.

#### Scenario: Command disables during execution

- **GIVEN** a rename operation is in progress
- **WHEN** the UI checks CanExecute
- **THEN** the command returns false
- **AND** the UI disables the rename button/menu item

#### Scenario: Command re-enables after completion

- **GIVEN** a rename operation completes
- **WHEN** the UI checks CanExecute
- **THEN** the command returns true
- **AND** the UI enables the rename button/menu item

### Requirement: Error Handling and Feedback

The ViewModel SHALL provide structured error information for failed rename operations.

#### Scenario: Expose error message for UI binding

- **WHEN** a rename operation fails
- **THEN** the ViewModel exposes an ErrorMessage property
- **AND** the error message is suitable for display to end users
- **AND** the error message includes actionable guidance (e.g., "A portfolio with this name already exists. Please choose a different name.")

#### Scenario: Clear error state on new operation

- **GIVEN** a failed rename with an error message
- **WHEN** a new rename operation begins
- **THEN** the previous error message is cleared
- **AND** the UI shows no error until the new operation completes

### Requirement: Validation Rules

The ViewModel SHALL validate new entity names before attempting the rename operation.

#### Scenario: Reject empty names

- **WHEN** RenameCommand is invoked with an empty or whitespace-only name
- **THEN** the validation fails
- **AND** an error message indicates "Name cannot be empty"
- **AND** the persistence layer is not invoked

#### Scenario: Reject names with invalid characters

- **WHEN** RenameCommand is invoked with a name containing `<>:"/\|?*`
- **THEN** the validation fails
- **AND** an error message indicates which characters are invalid
- **AND** the persistence layer is not invoked

#### Scenario: Trim whitespace from names

- **WHEN** RenameCommand is invoked with "  NewName  "
- **THEN** the name is trimmed to "NewName"
- **AND** the trimmed name is used for the rename operation
