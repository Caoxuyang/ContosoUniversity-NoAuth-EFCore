# Task 012 - Migrate Application Startup from Global.asax to Program.cs

## Summary

Migrated the ASP.NET MVC 5 application startup to the ASP.NET Core minimal hosting model.

## Changes Made

### Files Created
- `Program.cs` - New ASP.NET Core startup file using `WebApplicationBuilder` pattern
  - Registers `SchoolContext` DbContext with SQL Server connection from configuration
  - Adds MVC controllers with views (`AddControllersWithViews`)
  - Initializes database via `DbInitializer.Initialize`
  - Configures middleware pipeline: HTTPS redirection, static files, routing, authorization
  - Maps default controller route: `{controller=Home}/{action=Index}/{id?}`

### Files Deleted
- `Global.asax` - Replaced by `Program.cs`
- `Global.asax.cs` - Replaced by `Program.cs`
- `App_Start/RouteConfig.cs` - Route config migrated to `MapControllerRoute` in `Program.cs`
- `App_Start/FilterConfig.cs` - Filter config replaced by `AddControllersWithViews` in `Program.cs`
- `App_Start/BundleConfig.cs` - Bundle config replaced by static file references in `_Layout.cshtml`
- `Views/Web.config` - ASP.NET MVC 5 specific, replaced by `Views/_ViewImports.cshtml`

### Files Modified
- `ContosoUniversity.csproj` - Removed legacy `<Reference>` items (System.Web, System.Messaging, etc.), removed `Global.asax` compile items and script/content entries for moved files

## Middleware Pipeline (Task 019)
The `Program.cs` includes the complete ASP.NET Core middleware pipeline for a NoAuth application:
```
UseHttpsRedirection → UseStaticFiles → UseRouting → UseAuthorization → MapControllerRoute
```
