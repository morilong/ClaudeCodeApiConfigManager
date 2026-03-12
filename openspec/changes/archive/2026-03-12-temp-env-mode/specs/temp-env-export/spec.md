## ADDED Requirements

### Requirement: Export environment variables in shell-specific format

The system SHALL output environment variable commands in the correct format for the detected shell type.

#### Scenario: PowerShell format output
- **WHEN** shell type is PowerShell and temp export is requested
- **THEN** output format SHALL be `$env:ANTHROPIC_BASE_URL="value"` per line

#### Scenario: Bash/Zsh/Git Bash format output
- **WHEN** shell type is Bash, Zsh, or Git Bash and temp export is requested
- **THEN** output format SHALL be `export ANTHROPIC_BASE_URL="value"` per line

#### Scenario: Fish format output
- **WHEN** shell type is Fish and temp export is requested
- **THEN** output format SHALL be `set -x ANTHROPIC_BASE_URL "value"` per line

#### Scenario: CMD format output
- **WHEN** shell type is CMD and temp export is requested
- **THEN** output format SHALL be single line `set VAR1=val1 && set VAR2=val2 && ...`

### Requirement: Escape special characters in values

The system SHALL properly escape special characters in environment variable values.

#### Scenario: Value contains double quotes
- **WHEN** an environment variable value contains double quotes
- **THEN** the quotes SHALL be escaped appropriately for the target shell

#### Scenario: Value contains dollar sign
- **WHEN** an environment variable value contains `$` character
- **THEN** it SHALL be escaped to prevent variable expansion

### Requirement: Support standard environment variables

The system SHALL export the following standard environment variables:
- `ANTHROPIC_AUTH_TOKEN` or `ANTHROPIC_API_KEY`
- `ANTHROPIC_BASE_URL`
- `ANTHROPIC_MODEL`

#### Scenario: Export all standard variables
- **WHEN** temp export is requested for a valid config
- **THEN** all three standard variables SHALL be included in output

#### Scenario: Export custom parameters
- **WHEN** config has custom parameters
- **THEN** custom parameters SHALL also be exported as environment variables
