# UI Components - Spec Delta

## ADDED Requirements

### Requirement: ObservableDataView Rename Context Menu

The ObservableDataView component SHALL provide a "Rename" option in the context menu for each data row.

#### Scenario: Context menu includes rename option

- **GIVEN** an ObservableDataView displaying workspace entities
- **WHEN** the user right-clicks on a data row
- **THEN** a context menu appears
- **AND** the menu includes a "Rename" option
- **AND** the option is enabled if the entity can be renamed

#### Scenario: Rename option disabled for non-renameable entities

- **GIVEN** an ObservableDataView where the ViewModel does not support rename
- **WHEN** the user right-clicks on a data row
- **THEN** the "Rename" option is either disabled or not shown

#### Scenario: Rename option shows current name

- **GIVEN** a portfolio with name "MyPortfolio"
- **WHEN** the user clicks the Rename context menu option
- **THEN** a rename dialog appears
- **AND** the dialog shows the current name "MyPortfolio" in an editable field

### Requirement: Rename Dialog

The UI SHALL present a modal dialog for entering the new entity name.

#### Scenario: Dialog displays current name as placeholder or default

- **GIVEN** an entity with current name "OldName"
- **WHEN** the rename dialog opens
- **THEN** the text field contains "OldName" with the text selected
- **AND** the user can immediately type to replace the name

#### Scenario: Dialog validates input in real-time

- **WHEN** the user types an invalid character in the rename field
- **THEN** validation feedback appears below the text field
- **AND** the OK button is disabled

#### Scenario: Dialog confirms rename on OK

- **GIVEN** the user has entered a valid new name "NewName"
- **WHEN** the user clicks the OK button
- **THEN** the rename operation is triggered
- **AND** the dialog shows a loading indicator
- **AND** the dialog prevents further input during the operation

#### Scenario: Dialog cancels rename on Cancel

- **GIVEN** the rename dialog is open
- **WHEN** the user clicks the Cancel button or presses Escape
- **THEN** the dialog closes
- **AND** no rename operation is performed

### Requirement: Success and Error Feedback

The UI SHALL provide visual feedback for successful and failed rename operations.

#### Scenario: Show success notification

- **WHEN** a rename operation succeeds
- **THEN** the dialog closes
- **AND** a success notification (toast/snackbar) appears
- **AND** the notification displays "Renamed to [NewName]"
- **AND** the data grid updates to show the new name

#### Scenario: Show error message in dialog

- **WHEN** a rename operation fails
- **THEN** the dialog remains open
- **AND** an error message appears below the text field in red
- **AND** the error message shows the specific error (e.g., "A portfolio with this name already exists")
- **AND** the text field remains editable for correction

#### Scenario: Retry after error

- **GIVEN** a failed rename with an error message
- **WHEN** the user corrects the name and clicks OK again
- **THEN** a new rename operation is attempted
- **AND** the previous error message is cleared

### Requirement: Keyboard Navigation

The rename dialog SHALL support standard keyboard shortcuts for efficiency.

#### Scenario: Enter key confirms rename

- **GIVEN** the rename dialog is open with a valid name
- **WHEN** the user presses Enter
- **THEN** the rename operation is triggered

#### Scenario: Escape key cancels rename

- **GIVEN** the rename dialog is open
- **WHEN** the user presses Escape
- **THEN** the dialog closes without renaming

### Requirement: Loading State

The UI SHALL indicate when a rename operation is in progress.

#### Scenario: Show loading indicator during rename

- **WHEN** a rename operation is in progress
- **THEN** a loading spinner appears in the dialog
- **AND** the text field is disabled
- **AND** the OK and Cancel buttons are disabled
- **AND** the dialog cannot be closed by clicking outside

#### Scenario: Hide loading indicator after completion

- **WHEN** the rename operation completes (success or failure)
- **THEN** the loading spinner disappears
- **AND** the controls are re-enabled
- **AND** appropriate feedback is shown (success notification or error message)

### Requirement: Accessibility

The rename dialog SHALL be accessible to keyboard and screen reader users.

#### Scenario: Dialog has proper focus management

- **WHEN** the rename dialog opens
- **THEN** focus is placed in the name text field
- **AND** the current name text is selected

#### Scenario: Error messages are announced to screen readers

- **WHEN** a validation error or rename failure occurs
- **THEN** the error message has appropriate ARIA attributes
- **AND** screen readers announce the error message

#### Scenario: Dialog has accessible labels

- **GIVEN** the rename dialog
- **THEN** the text field has an accessible label "New name"
- **AND** the OK button has label "Rename"
- **AND** the Cancel button has label "Cancel"
