# Task 009 - Handle jQuery and Client-Side Library Packages

## Status: Success

## Summary
Removed client-side library packages from NuGet. These should be managed via wwwroot directly or via libman in a future step.

## Changes Made

### ContosoUniversity.csproj
- Removed `jQuery` Version 3.7.1
- Removed `jQuery.Validation` Version 1.21.0
- Removed `Microsoft.jQuery.Unobtrusive.Validation` Version 4.0.0
- Removed `bootstrap` Version 5.3.3
- Removed `Modernizr` Version 2.6.2

## Notes
- Client-side static assets (JS/CSS) already exist in the `Scripts/` and `Content/` folders.
- A future task (018-upgrade-migrate-static-files) will move these to `wwwroot/` and update references in `_Layout.cshtml`.
- libman or npm can be used to manage client-side dependencies going forward.
