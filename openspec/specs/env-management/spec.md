# env-management Specification

## Purpose
TBD - created by archiving change temp-env-mode. Update Purpose after archive.
## Requirements
### Requirement: Default to temporary environment variable mode

The system SHALL set temporary environment variables by default when using `ccm use`.

#### Scenario: Default temp mode
- **WHEN** user runs `ccm use <name>` without any mode flag
- **THEN** environment variables SHALL be output for eval (temporary, current terminal only)

#### Scenario: Update active config in temp mode
- **WHEN** user runs `ccm use <name>` in temp mode
- **THEN** `activeConfigName` in `settings.json` SHALL be updated

### Requirement: Support --persist flag for permanent environment variables

The system SHALL support `-p`/`--persist` flag for permanent environment variable mode.

#### Scenario: Persist mode with short flag
- **WHEN** user runs `ccm use <name> -p`
- **THEN** environment variables SHALL be permanently set AND output for current terminal

#### Scenario: Persist mode with long flag
- **WHEN** user runs `ccm use <name> --persist`
- **THEN** environment variables SHALL be permanently set AND output for current terminal

### Requirement: Persist mode sets both permanent and temporary

The system SHALL set permanent environment variables AND output temporary commands in persist mode.

#### Scenario: Persist mode dual output
- **WHEN** user runs `ccm use <name> --persist`
- **THEN** permanent environment variables SHALL be written (Windows registry / Unix shell script)
- **AND** temporary commands SHALL be output for current terminal eval

#### Scenario: Current terminal effective immediately
- **WHEN** persist mode output is eval'd
- **THEN** environment variables SHALL be effective in current terminal immediately

#### Scenario: New terminal effective automatically
- **WHEN** a new terminal is opened after persist mode
- **THEN** environment variables SHALL be effective automatically

