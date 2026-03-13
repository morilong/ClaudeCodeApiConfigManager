# shell-function-injection Delta Specification

## ADDED Requirements

### Requirement: Inject ccm-claude/ccm-c launcher function during install

The system SHALL inject `ccm-claude` and `ccm-c` launcher functions alongside the existing `ccm` wrapper function.

#### Scenario: PowerShell launcher injection
- **WHEN** installing on Windows with PowerShell available
- **THEN** `ccm-claude` and `ccm-c` functions SHALL be added to `$PROFILE`

#### Scenario: Bash/Zsh launcher injection
- **WHEN** installing on Unix with Bash or Zsh
- **THEN** `ccm-claude` and `ccm-c` functions SHALL be added to `~/.bashrc` or `~/.zshrc`

#### Scenario: Fish launcher injection
- **WHEN** installing on Unix with Fish
- **THEN** `ccm-claude` and `ccm-c` functions SHALL be added to `~/.config/fish/config.fish`

#### Scenario: Git Bash launcher injection
- **WHEN** installing on Windows with Git Bash available
- **THEN** `ccm-claude` and `ccm-c` functions SHALL be added to `~/.bashrc`

### Requirement: Remove launcher functions during uninstall

The system SHALL remove `ccm-claude` and `ccm-c` functions during uninstallation.

#### Scenario: Remove launcher from all shells
- **WHEN** uninstalling
- **THEN** `ccm-claude` and `ccm-c` function blocks SHALL be removed from all injected config files
