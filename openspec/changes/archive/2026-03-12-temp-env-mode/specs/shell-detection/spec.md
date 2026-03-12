## ADDED Requirements

### Requirement: Detect Git Bash on Windows

The system SHALL detect when running in Git Bash by checking the `MSYSTEM` environment variable.

#### Scenario: Git Bash detected
- **WHEN** `MSYSTEM` environment variable exists (MINGW64, MINGW32, or MSYS)
- **THEN** shell type SHALL be detected as GitBash

#### Scenario: Not Git Bash
- **WHEN** `MSYSTEM` environment variable does not exist
- **THEN** continue to check other shell types

### Requirement: Detect Unix shell types

The system SHALL detect Unix shell types by parsing the `SHELL` environment variable.

#### Scenario: Fish shell detected
- **WHEN** `$SHELL` contains "fish"
- **THEN** shell type SHALL be detected as Fish

#### Scenario: Zsh detected
- **WHEN** `$SHELL` contains "zsh"
- **THEN** shell type SHALL be detected as Zsh

#### Scenario: Bash detected
- **WHEN** `$SHELL` contains "bash"
- **THEN** shell type SHALL be detected as Bash

### Requirement: Detect Windows shell types

The system SHALL detect Windows shell types when running on Windows.

#### Scenario: PowerShell detected
- **WHEN** running on Windows AND `PSModulePath` environment variable exists
- **THEN** shell type SHALL be detected as PowerShell

#### Scenario: CMD detected
- **WHEN** running on Windows AND no other Windows shell type detected
- **THEN** shell type SHALL be detected as CMD

### Requirement: Detection priority order

The system SHALL detect shell types in the following priority order:
1. Git Bash (check `MSYSTEM`)
2. Fish (check `$SHELL`)
3. Zsh (check `$SHELL`)
4. Bash (check `$SHELL`)
5. PowerShell (Windows + `PSModulePath`)
6. CMD (Windows fallback)

#### Scenario: Git Bash on Windows takes priority
- **WHEN** running on Windows with Git Bash AND `MSYSTEM` is set
- **THEN** shell type SHALL be GitBash (not PowerShell or CMD)

### Requirement: Return shell type enum

The system SHALL return a `ShellType` enum value from detection.

#### Scenario: Enum values available
- **WHEN** shell detection is performed
- **THEN** result SHALL be one of: `PowerShell`, `Cmd`, `GitBash`, `Bash`, `Zsh`, `Fish`
