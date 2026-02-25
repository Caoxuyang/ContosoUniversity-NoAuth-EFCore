# Task 011: Restore Packages and Build - Modernization Summary

## Status
**Success** (build failure expected - goal was to document errors)

## Overview
- `dotnet restore` completed successfully.
- `dotnet build` failed with **141 errors** (expected).
- All errors are due to ASP.NET MVC 5 / System.Web APIs being unavailable in .NET 9.0.

---

## Build Error Categories

### 1. Namespace Changes – `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`
All controllers and views still reference `System.Web.Mvc` which does not exist in .NET 9.0.

**Files affected:**
- `Controllers/BaseController.cs`
- `Controllers/CoursesController.cs`
- `Controllers/DepartmentsController.cs`
- `Controllers/HomeController.cs`
- `Controllers/InstructorsController.cs`
- `Controllers/NotificationsController.cs`
- `Controllers/StudentsController.cs`
- `Views/Shared/Error.cshtml`
- `Global.asax.cs`
- `App_Start/FilterConfig.cs`
- `App_Start/RouteConfig.cs`

**Error:** `CS0234: The type or namespace name 'Mvc' does not exist in the namespace 'System.Web'`

### 2. Namespace Changes – `System.Web.Routing` → `Microsoft.AspNetCore.Routing`
**Files affected:**
- `App_Start/RouteConfig.cs`
- `Global.asax.cs`

**Error:** `CS0234: The type or namespace name 'Routing' does not exist in the namespace 'System.Web'`

### 3. Namespace Changes – `System.Web.Optimization` (Bundle) removed
**Files affected:**
- `App_Start/BundleConfig.cs`
- `Global.asax.cs`

**Error:** `CS0234: The type or namespace name 'Optimization' does not exist in the namespace 'System.Web'`

### 4. Type Removals – ASP.NET MVC 5 Types Not Available in ASP.NET Core
These types must be replaced with their ASP.NET Core equivalents:

| Removed Type | Replacement |
|---|---|
| `ActionResult` | `Microsoft.AspNetCore.Mvc.ActionResult` or `IActionResult` |
| `JsonResult` | `Microsoft.AspNetCore.Mvc.JsonResult` |
| `Controller` (base) | `Microsoft.AspNetCore.Mvc.Controller` |
| `HttpPost` / `HttpPostAttribute` | `Microsoft.AspNetCore.Mvc.HttpPostAttribute` |
| `HttpGet` / `HttpGetAttribute` | `Microsoft.AspNetCore.Mvc.HttpGetAttribute` |
| `ValidateAntiForgeryToken` | `Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute` |
| `Bind` / `BindAttribute` | `Microsoft.AspNetCore.Mvc.BindAttribute` |
| `ActionName` / `ActionNameAttribute` | `Microsoft.AspNetCore.Mvc.ActionNameAttribute` |
| `GlobalFilterCollection` | `Microsoft.AspNetCore.Mvc.Filters` |
| `RouteCollection` | `Microsoft.AspNetCore.Routing` |
| `BundleCollection` | Removed – use static files in wwwroot |
| `HttpPostedFileBase` | `Microsoft.AspNetCore.Http.IFormFile` |
| `HttpApplication` | Removed – use Program.cs |

### 5. Type Removals – `System.Messaging` (MSMQ)
**Files affected:** `Services/NotificationService.cs`

`System.Messaging.MessageQueue` is not available in .NET 9.0.

**Error:** `CS0234: The type or namespace name 'Messaging' does not exist in the namespace 'System'`

**Fix:** Remove or replace MSMQ-based notification with a supported messaging alternative.

### 6. Missing Type – `PaginatedList<>`
**Files affected:**
- `Views/Students/Index.cshtml`

`PaginatedList<>` is not resolved in the view context. Ensure `PaginatedList.cs` is properly included and the view's `@model` namespace is correct.

**Error:** `CS0246: The type or namespace name 'PaginatedList<>' could not be found`

### 7. App_Start Folder – Legacy ASP.NET MVC 5 Helpers
The entire `App_Start` folder uses `System.Web.*` APIs:
- `BundleConfig.cs` – `System.Web.Optimization`
- `FilterConfig.cs` – `System.Web.Mvc`
- `RouteConfig.cs` – `System.Web.Mvc` and `System.Web.Routing`

These must be migrated to `Program.cs` and deleted.

### 8. Global.asax – Removed in ASP.NET Core
`Global.asax.cs` uses `System.Web.Mvc`, `System.Web.Optimization`, `System.Web.Routing`, and `HttpApplication` which are all unavailable.

Must be replaced by `Program.cs`.

---

## Error Count by File

| File | Error Count |
|---|---|
| Controllers/CoursesController.cs | ~22 |
| Controllers/DepartmentsController.cs | ~18 |
| Controllers/StudentsController.cs | ~17 |
| Controllers/InstructorsController.cs | ~16 |
| Controllers/HomeController.cs | 6 |
| Controllers/NotificationsController.cs | 5 |
| Controllers/BaseController.cs | 2 |
| App_Start/RouteConfig.cs | 3 |
| App_Start/FilterConfig.cs | 2 |
| App_Start/BundleConfig.cs | 2 |
| Global.asax.cs | 4 |
| Services/NotificationService.cs | 2 |
| Views/Shared/Error.cshtml | 1 |
| Views/Students/Index.cshtml | 1 |
| **Total** | **141** |

---

## Next Steps
The following tasks will address these errors:
- **Task 012**: Migrate `Global.asax` to `Program.cs` (removes Global.asax errors + App_Start errors)
- **Task 015**: Update controllers to use `Microsoft.AspNetCore.Mvc` namespace
- **Task 016**: Update views to use Tag Helpers and correct namespaces
- **Task 018**: Migrate static files (removes BundleConfig dependency)
- **Task 020**: Fix remaining compilation errors
