# .NET Upgrade Plan Template

## Schema Rules

- Use `upgradeTask` type from `tasks-schema.json`
- `successCriteria` values: **strings** (`"true"`, `"false"`)
- `skills.location`: `"builtin"` | `"project"` | `"remote"`
- `status`: `"pending"`

## Example: tasks.json

```json
{
  "$schema": "tasks-schema.json",
  "description": ".NET version upgrade plan from .NET Framework 4.8 to .NET 10.0",
  "tasks": [
    {
      "type": "upgrade",
      "id": "001-upgrade-sdk-style-conversion",
      "description": "Convert ContosoUniversity.csproj to SDK-style project format",
      "requirements": "Convert legacy non-SDK-style .csproj to modern SDK-style format. Do NOT change target framework. Remove packages.config after conversion. Verify project builds.",
      "dependencies": [],
      "environmentConfiguration": null,
      "skills": [{ "name": "sdk-style-conversion", "location": "builtin" }],
      "successCriteria": {
        "passBuild": "true",
        "generateNewUnitTests": "false",
        "generateNewIntegrationTests": "false",
        "passUnitTests": "true",
        "passIntegrationTests": "false",
        "securityComplianceCheck": "false"
      },
      "status": "pending"
    },
    {
      "type": "upgrade",
      "id": "002-upgrade-target-framework-packages",
      "description": "Update target framework to net10.0 and upgrade 26 NuGet packages",
      "requirements": "Change TargetFramework to net10.0. Change Sdk to Microsoft.NET.Sdk.Web. Remove 10 packages included in framework. Remove 2 incompatible packages. Upgrade 22 packages to .NET 10.0 compatible versions. Address Microsoft.Data.SqlClient security vulnerability by upgrading to 6.1.4.",
      "dependencies": ["001-upgrade-sdk-style-conversion"],
      "environmentConfiguration": null,
      "skills": [],
      "successCriteria": {
        "passBuild": "false",
        "generateNewUnitTests": "false",
        "generateNewIntegrationTests": "false",
        "passUnitTests": "false",
        "passIntegrationTests": "false",
        "securityComplianceCheck": "true"
      },
      "status": "pending"
    }
  ],
  "metadata": {
    "planName": "upgrade-to-lts",
    "projectName": "ContosoUniversity",
    "language": "dotnet",
    "createdAt": "2026-02-13T00:00:00.000Z",
    "version": "1.0"
  }
}
```

## Example: plan.md

```markdown
# Migration Plan: ContosoUniversity .NET Framework 4.8 → .NET 10.0 (LTS)

## Executive Summary

Migrating ContosoUniversity from ASP.NET MVC 5 on .NET Framework 4.8 to ASP.NET Core MVC on .NET 10.0 (LTS).
1 project, 3,392 LOC, 45 NuGet packages (26 need upgrade), 92 API incompatibilities.

**Upgrade Strategy:** All-At-Once — Single project, small codebase, clear scope.

## Task Execution Order

### TASK-001: Convert to SDK-style project
...

## Success Criteria

- [ ] Project targets net10.0
- [ ] `dotnet build` succeeds with 0 errors
- [ ] No security vulnerabilities in NuGet packages
```
