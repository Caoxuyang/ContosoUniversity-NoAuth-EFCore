# Task 025 - Cleanup of Obsolete Files and Configurations

## Summary

Removed obsolete .NET Framework artifacts and cleaned up the project configuration.

## Files Removed

| File | Reason |
|------|--------|
| `Web.config` | .NET Framework IIS configuration — ASP.NET Core uses `appsettings.json` and auto-generates `web.config` on publish |

## Files Not Found (Already Removed in Earlier Tasks)

- `packages.config` — removed during SDK-style conversion
- `Global.asax` / `Global.asax.cs` — replaced by `Program.cs`
- `App_Start/` folder — replaced by `Program.cs` startup

## Project File Changes (`ContosoUniversity.csproj`)

- Removed redundant `<OutputType>Exe</OutputType>` property group (implicit for `Microsoft.NET.Sdk.Web`)
- Removed `<Folder Include="App_Data\" />` (leftover .NET Framework artifact)
- Removed stray blank line in `PackageReference` group

## `.gitignore` Updates

- Fixed missing newlines between sections
- Added `.NET 9.0` build artifact patterns (`*.nupkg`, `*.snupkg`)
- Added `wwwroot/lib/` for LibMan client-side packages

## Build Verification

- `dotnet build` succeeds with 0 errors after cleanup
