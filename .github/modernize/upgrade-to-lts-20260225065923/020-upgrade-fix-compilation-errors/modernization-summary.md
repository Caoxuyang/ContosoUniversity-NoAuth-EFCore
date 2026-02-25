# Task 020: Fix Compilation Errors for .NET 9.0

## Status: Complete

## Summary
Fixed all compilation errors to make the project build successfully on .NET 9.0.

## Changes Made

### 1. Controllers — Removed duplicate old MVC5 code (4 files)
Each of the following controller files had an entire copy of the old ASP.NET MVC5 controller code accidentally appended after the closing `}` of the namespace. The duplicate code was removed:

- `Controllers/CoursesController.cs` — removed 249 lines of old MVC5 code (lines 258–506)
- `Controllers/DepartmentsController.cs` — removed 165 lines of old MVC5 code (lines 177–340)
- `Controllers/StudentsController.cs` — removed 218 lines of old MVC5 code (lines 229–445)
- `Controllers/InstructorsController.cs` — removed 248 lines of old MVC5 code (lines 262–508)

**Root cause**: CS8803 "Top-level statements must precede namespace and type declarations" — methods were outside any class/namespace scope.

### 2. Program.cs — Added missing using directives
Added explicit `using` statements required for ASP.NET Core top-level programs:
- `Microsoft.AspNetCore.Builder` (for `WebApplication`)
- `Microsoft.Extensions.Configuration` (for `GetConnectionString`)
- `Microsoft.Extensions.DependencyInjection` (for `AddDbContext`, `AddControllersWithViews`)
- `Microsoft.Extensions.Hosting` (for `IsDevelopment`)

### 3. Views/Instructors/Create.cshtml and Edit.cshtml — Fixed Razor syntax
Replaced old MVC5 pattern `Html.Raw(course.Assigned ? "checked=""checked""" : "")` with ASP.NET Core compatible `@(course.Assigned ? "checked" : "")`.

The old double-quote escape syntax (`""`) is invalid in C# string literals (valid only in VB.NET or HTML attribute strings).

## Build Result
**Build succeeded — 0 Errors, 1 Warning** (pre-existing warning in HomeController unrelated to this task).
