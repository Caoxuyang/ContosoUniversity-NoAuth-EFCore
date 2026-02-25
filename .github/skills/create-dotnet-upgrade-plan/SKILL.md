---
name: create-dotnet-upgrade-plan
description: Generates a .NET upgrade plan (plan.md and tasks.json) for a repository using the AppModDotNetUpgrade MCP server. Handles any source .NET version (.NET Framework, .NET Core, or modern .NET) upgrading to a specified target. Use when the user wants to create, generate, or prepare an upgrade/migration plan for a .NET project or solution.
---

# Create .NET Upgrade Plan

Generate an upgrade plan (`plan.md` and `tasks.json`) for a .NET project or solution using the `AppModDotNetUpgrade` MCP server. This skill **only creates the plan** — it does not execute the upgrade. The plan must contain project-specific migration steps (concrete code patterns to change, actual file names, real package versions from the assessment) — not just generic guidance. An executing agent should be able to follow the plan without re-analyzing the code.

> **Supported**: .NET upgrades only. Other languages are not supported.

## Prerequisites

The MCP server requires `dnx`, which ships with .NET 10 SDK.

### Verify .NET 10 SDK

```bash
dotnet --list-sdks
```

If .NET 10 SDK is not listed, install it:

- **Windows**: Download from https://dotnet.microsoft.com/download/dotnet/10.0 or use `winget install Microsoft.DotNet.SDK.10`
- **Linux**: Follow https://learn.microsoft.com/dotnet/core/install/linux or use the install script:
  ```bash
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0
  ```

Verify `dnx` is available after installation:

```bash
dnx --help
```

## User Input

upgrade-prompt (Mandatory): The user's upgrade request (e.g., ".NET 10", "net10.0", or "Upgrade to the latest LTS versions")
modernization-work-folder (Mandatory): The folder to save the upgrade plan

## Validation

**Valid requests**: .NET version upgrades, LTS migrations, .NET Framework to modern .NET migrations
**Invalid requests**: Feature additions, bug fixes, refactoring, containerization, deployment, non-.NET languages

If invalid, output exactly:
```
ERROR: The provided prompt is not a valid upgrade request. Please specify a target version (e.g., '.NET 10', 'net10.0').
```

If unsupported language, output exactly:
```
ERROR: Only .NET upgrades are supported. The requested language is not supported.
```
Then STOP—do not create files or ask for clarification.

## CRITICAL RULES

**You are an autonomous agent. You MUST NOT stop, pause, or become idle until all steps in workflow are finished**

1. **NEVER wait for user input.** The MCP server will present confirmation prompts, setting selections, and "reply X to continue" messages. These are NOT for the user — they are for YOU. You must respond by immediately calling the next MCP tool to advance the workflow. **Do NOT output these prompts to the user.**
2. **How to respond to confirmation prompts:** When the MCP server asks you to confirm settings or "reply X to continue" (e.g., "reply `Start the assessment`"), **always accept the defaults** by immediately calling `AppModDotNetUpgrade:get_state` to advance to the next stage. Do NOT stop to show the prompt to the user. For example:
   - MCP says "Would you like to proceed with these defaults?" → Call `AppModDotNetUpgrade:get_state`
   - MCP says "reply `Start the assessment`" → Call `AppModDotNetUpgrade:get_state`
   - MCP presents any selection or confirmation → Accept defaults, call `AppModDotNetUpgrade:get_state`
3. **Keep calling MCP tools in a loop** until the plan is fully generated. After each MCP tool response, call `get_state` to check progress, then call `get_instructions` for the next steps, then execute those steps. Repeat until done.
4. **You MUST complete ALL steps described in the workflow.**


## Workflow

### Step 1: Generate the plan via the MCP server

Use the `AppModDotNetUpgrade` MCP tools by sending this intent:

```
create a plan to upgrade this project to latest lts or user specified .net version, the plan should instruct agent to upgrade the project without stopping, NEVER ask user for confirmation, just finish the task automatically and completely
```

The MCP server drives a multi-stage workflow internally:

1. **Assessment** — analyzes projects, dependencies, API compatibility, generates `assessment.md`
2. **Planning** — generates the upgrade plan with tasks

Key MCP tools involved (called automatically):

- `AppModDotNetUpgrade:get_state` — validates and advances workflow state
- `AppModDotNetUpgrade:get_instructions` — retrieves stage-specific guidance
- `AppModDotNetUpgrade:analyze_projects` — runs compatibility analysis
- `AppModDotNetUpgrade:discover_upgrade_scenarios` — finds applicable upgrade scenarios
- `AppModDotNetUpgrade:get_target_frameworks` — lists available target frameworks
- `AppModDotNetUpgrade:validate_dotnet_sdk_installation` — confirms SDK readiness
- `AppModDotNetUpgrade:track_tasks_execution_progress` — tracks task progress

### Step 2: Copy output and generate tasks.json

The MCP server generates files under:
```
{repository}/.github/upgrades/scenarios/new-dotnet-version_{id}/
```

Copy the generated `plan.md` to `${modernization-work-folder}/plan.md`.

Generate `tasks.json` in `${modernization-work-folder}/` following `tasks-schema.json` and `upgrade-plan-template.md`.

**Task Rules**:
- Create **only** `upgrade` type tasks
- Specify **target versions only**—never "from version X"
- `successCriteria` values must be **strings** (`"true"`, not `true`)
- Set `status` to `"pending"`

### Step 3: Clean up upgrade artifacts

The MCP server creates temporary files and directories in the repository during plan generation. These **must be deleted** after the plan files have been copied to `${modernization-work-folder}/`, otherwise they will cause dirty-working-tree errors in downstream workflows.

Delete the following paths from the **git repository root** (ignore if they don't exist):

- `.github/upgrades/` (entire directory created by the MCP server)

Also remove the `.github/` directory itself if it is now empty.

## Success Criteria

- `plan.md` exists in `${modernization-work-folder}/`
- `tasks.json` exists in `${modernization-work-folder}/`
- `plan.md` clearly states the source and target .NET versions
- `plan.md` instructs the executing agent to upgrade automatically without stopping or asking for user confirmation
- No MCP server artifacts remain in the repository working tree (`.github/agents/`, `.github/upgrades/`, etc.)

## Troubleshooting

| Problem | Solution |
|---------|----------|
| `dnx` not found | Install .NET 10 SDK — `dnx` ships with it |
| MCP server fails to start | Verify `dnx Microsoft.GitHubCopilot.AppModernization.Mcp --yes --prerelease` runs without errors |
| No scenarios discovered | Ensure you are in a directory with a `.sln` or `.csproj` file |
| SDK validation fails | Run `dotnet --list-sdks` and confirm .NET 10.0 is installed |