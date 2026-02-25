# Task 002: Upgrade Target Framework to net9.0

## Summary

Updated the project file `ContosoUniversity.csproj` to target .NET 9.0.

## Changes Made

**File:** `ContosoUniversity.csproj`

| Property | Before | After |
|---|---|---|
| `TargetFramework` | `net48` | `net9.0` |
| `OutputType` | `Library` | `Exe` |
| `Project Sdk` | `Microsoft.NET.Sdk.Web` | `Microsoft.NET.Sdk.Web` (already correct) |

## Notes

- The SDK was already set to `Microsoft.NET.Sdk.Web` — no change needed.
- `OutputType` changed from `Library` to `Exe` as required for a .NET 9 web application entry point.
- No build was performed as per task requirements (`passBuild=false`).
