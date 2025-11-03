# Data Persistence - Spec Delta

## ADDED Requirements

### Requirement: Entity Rename Operation

The reactive persistence layer SHALL provide a rename operation for workspace entities that atomically updates both the in-memory cache and filesystem storage.

#### Scenario: Rename flat file entity successfully

- **GIVEN** a bot entity stored as `BotEntitys/OldName.bot.hjson`
- **WHEN** rename is requested from "OldName" to "NewName"
- **THEN** the file is renamed to `BotEntitys/NewName.bot.hjson`
- **AND** the entity's Key property is updated to "NewName" in memory
- **AND** the reactive cache is updated with the new key
- **AND** a success result is returned

#### Scenario: Rename directory-based entity successfully

- **GIVEN** a portfolio entity stored as `Portfolio2s/OldName/portfolio.hjson`
- **WHEN** rename is requested from "OldName" to "NewName"
- **THEN** the directory is renamed to `Portfolio2s/NewName/`
- **AND** the entity's Key property is updated to "NewName" in memory
- **AND** the reactive cache is updated with the new key
- **AND** a success result is returned

#### Scenario: Prevent duplicate names

- **GIVEN** entities with keys "Portfolio1" and "Portfolio2"
- **WHEN** rename is requested from "Portfolio1" to "Portfolio2"
- **THEN** the rename operation fails
- **AND** an error result is returned indicating the name already exists
- **AND** no filesystem changes are made
- **AND** the cache remains unchanged

#### Scenario: Handle invalid filenames

- **GIVEN** an entity with key "ValidName"
- **WHEN** rename is requested to a name containing invalid filesystem characters (e.g., `<>:"/\|?*`)
- **THEN** the rename operation fails
- **AND** an error result is returned indicating invalid characters
- **AND** no filesystem changes are made

#### Scenario: Handle filesystem errors

- **GIVEN** an entity with key "Locked"
- **WHEN** rename is requested while the file is locked by another process
- **THEN** the rename operation fails
- **AND** an error result is returned with the underlying IOException details
- **AND** the cache remains unchanged

### Requirement: Rename Result Type

The persistence layer SHALL return a structured result type that indicates success or failure with detailed error information.

#### Scenario: Successful rename returns new key

- **WHEN** a rename operation succeeds
- **THEN** the result contains `Success = true`
- **AND** the result contains the new key value
- **AND** the result contains `Error = null`

#### Scenario: Failed rename returns error details

- **WHEN** a rename operation fails
- **THEN** the result contains `Success = false`
- **AND** the result contains `Error` with the error message
- **AND** the result may contain `ErrorCode` for programmatic handling (e.g., "DuplicateName", "InvalidCharacters", "FileLocked")

### Requirement: Storage Mode Detection

The persistence layer SHALL automatically detect whether an entity uses flat file or directory-based storage and perform the appropriate rename operation.

#### Scenario: Detect directory storage mode

- **GIVEN** an entity type with `[Vos(VosFlags.PreferDirectory)]` attribute
- **WHEN** a rename is requested
- **THEN** the system renames the entity's directory

#### Scenario: Detect flat file storage mode

- **GIVEN** an entity type without `VosFlags.PreferDirectory`
- **WHEN** a rename is requested
- **THEN** the system renames the entity's file

### Requirement: Atomic Operations

Rename operations SHALL be atomic at the filesystem level to prevent data corruption.

#### Scenario: Filesystem rename is atomic

- **WHEN** a rename operation is performed
- **THEN** the underlying filesystem operation uses an atomic rename
- **AND** the entity is either fully renamed or not renamed at all
- **AND** partial renames are not possible

#### Scenario: Cache update follows filesystem success

- **WHEN** a rename operation is performed
- **THEN** the filesystem operation completes first
- **AND** the cache is updated only if the filesystem operation succeeds
- **AND** if the filesystem operation fails, the cache remains unchanged
