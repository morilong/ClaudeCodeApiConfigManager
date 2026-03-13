# claude-launcher Specification

## Purpose
提供快捷命令 `ccm-claude`/`ccm-c`，一键切换配置并启动 Claude Code。

## ADDED Requirements

### Requirement: Basic claude launcher command

The system SHALL provide a `ccm-claude` shell function that switches config and launches Claude.

#### Scenario: Launch claude with config
- **WHEN** user runs `ccm-claude zhipu`
- **THEN** system SHALL switch to config "zhipu" and execute `claude`

#### Scenario: Alias ccm-c
- **WHEN** user runs `ccm-c zhipu`
- **THEN** system SHALL behave identically to `ccm-claude zhipu`

### Requirement: Auto-skip permissions mode

The system SHALL support `-y` flag to skip permission prompts.

#### Scenario: Skip permissions mode
- **WHEN** user runs `ccm-claude zhipu -y` or `ccm-c zhipu -y`
- **THEN** system SHALL execute `claude --dangerously-skip-permissions`

#### Scenario: Flag can appear anywhere after config name
- **WHEN** user runs `ccm-claude -y zhipu`
- **THEN** system SHALL correctly parse and apply skip permissions mode

### Requirement: Error handling for missing config

The system SHALL handle cases where config does not exist.

#### Scenario: Config not found
- **WHEN** user runs `ccm-claude nonexistent`
- **THEN** system SHALL output error message from `ccm use` and NOT launch claude

### Requirement: Pass additional arguments to claude

The system SHALL allow passing additional arguments to claude.

#### Scenario: Additional claude arguments
- **WHEN** user runs `ccm-claude zhipu --print some-prompt`
- **THEN** system SHALL execute `claude --print some-prompt` with config "zhipu"
