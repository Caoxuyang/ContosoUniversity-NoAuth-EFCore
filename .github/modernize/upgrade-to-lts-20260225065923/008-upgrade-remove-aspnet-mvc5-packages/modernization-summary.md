# Task 008 - Remove ASP.NET MVC 5 Packages

## Status: Success

## Summary
Removed ASP.NET MVC 5 packages replaced by ASP.NET Core MVC included in Microsoft.NET.Sdk.Web.

## Changes Made

### ContosoUniversity.csproj
- Removed `Microsoft.AspNet.Mvc` Version 5.2.9
- Removed `Microsoft.AspNet.Razor` Version 3.2.9
- Removed `Microsoft.AspNet.WebPages` Version 3.2.9

## Notes
These packages are replaced by ASP.NET Core MVC framework provided by `Microsoft.NET.Sdk.Web` SDK. No explicit package addition is needed.
