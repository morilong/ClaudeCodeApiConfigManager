# shell-function-injection Specification

## Purpose
TBD - created by archiving change temp-env-mode. Update Purpose after archive.
## Requirements
### Requirement: Inject PowerShell function during install

The system SHALL inject a wrapper function into the PowerShell `$PROFILE` during installation.

#### Scenario: PowerShell profile injection
- **WHEN** installing on Windows with PowerShell available
- **THEN** a ccm wrapper function SHALL be added to `$PROFILE`

#### Scenario: PowerShell function behavior
- **WHEN** user runs `ccm use xxx` in PowerShell
- **THEN** the function SHALL intercept the command and pipe output to `Invoke-Expression`

#### Scenario: Skip if already injected
- **WHEN** ccm marker comment already exists in `$PROFILE`
- **THEN** injection SHALL be skipped

### Requirement: Inject Bash/Zsh function during install

The system SHALL inject a wrapper function into `.bashrc` or `.zshrc` during installation.

#### Scenario: Bash function injection
- **WHEN** installing on Unix with Bash
- **THEN** a ccm wrapper function SHALL be added to `~/.bashrc`

#### Scenario: Zsh function injection
- **WHEN** installing on Unix with Zsh
- **THEN** a ccm wrapper function SHALL be added to `~/.zshrc`

#### Scenario: Bash/Zsh function behavior
- **WHEN** user runs `ccm use xxx` in Bash/Zsh
- **THEN** the function SHALL intercept the command and use `eval` to execute output

### Requirement: Inject Fish function during install

The system SHALL inject a wrapper function into Fish config during installation.

#### Scenario: Fish function injection
- **WHEN** installing on Unix with Fish
- **THEN** a ccm wrapper function SHALL be added to `~/.config/fish/config.fish`

#### Scenario: Fish function behavior
- **WHEN** user runs `ccm use xxx` in Fish
- **THEN** the function SHALL intercept the command and use `eval` to execute output

### Requirement: Inject Git Bash function during install

The system SHALL inject a wrapper function into Git Bash `.bashrc` during installation.

#### Scenario: Git Bash function injection
- **WHEN** installing on Windows with Git Bash available
- **THEN** a ccm wrapper function SHALL be added to `~/.bashrc` (Windows user home)

### Requirement: Function handles --persist flag

The wrapper function SHALL handle the `--persist`/`-p` flag differently from default behavior.

#### Scenario: Default behavior (temp mode)
- **WHEN** user runs `ccm use xxx` without `--persist`
- **THEN** function SHALL append `--temp` flag and eval output

#### Scenario: Persist mode
- **WHEN** user runs `ccm use xxx --persist` or `ccm use xxx -p`
- **THEN** function SHALL pass through and eval output (ccm handles both temp and persist)

### Requirement: Remove function during uninstall

The system SHALL remove injected shell functions during uninstallation.

#### Scenario: Remove from PowerShell profile
- **WHEN** uninstalling
- **THEN** ccm function block SHALL be removed from `$PROFILE`

#### Scenario: Remove from Bash/Zsh/Fish configs
- **WHEN** uninstalling
- **THEN** ccm function block SHALL be removed from all injected config files

### Requirement: Use marker comments for identification

The system SHALL use marker comments to identify injected code blocks.

#### Scenario: Marker format
- **WHEN** injecting shell functions
- **THEN** code block SHALL be wrapped with `# <ccm-init>` and `# </ccm-init>` markers

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

