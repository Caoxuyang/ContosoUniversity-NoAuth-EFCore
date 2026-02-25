# Task 015 - Update Controller and Action Attributes

## Summary

Updated all controllers from ASP.NET MVC 5 to ASP.NET Core patterns.

## Changes Made

### Files Modified
All controllers updated with the following changes:

- **Namespace**: `using System.Web.Mvc` → `using Microsoft.AspNetCore.Mvc`
- **Removed**: `using System.Web`, `using System.Net` (HttpStatusCode)
- **Return type**: `ActionResult` → `IActionResult`
- **BadRequest**: `new HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()`
- **NotFound**: `HttpNotFound()` → `NotFound()`
- **Bind attribute**: `[Bind(Include = "...")]` → `[Bind("...")]`
- **JSON**: `Json(..., JsonRequestBehavior.AllowGet)` → `Json(...)`
- **File upload**: `HttpPostedFileBase` → `IFormFile`; `teachingMaterialImage.ContentLength` → `teachingMaterialImage.Length`
- **File save**: `SaveAs()` → `CopyTo(Stream)`
- **File path**: `Server.MapPath("~/...")` → `IWebHostEnvironment.WebRootPath`
- **SelectList**: `new SelectList(...)` → `new Microsoft.AspNetCore.Mvc.Rendering.SelectList(...)`
- **TryUpdateModel**: Sync MVC5 method → `async TryUpdateModelAsync<T>` (InstructorsController)

### Controllers Updated
- `BaseController.cs` - Base class updated
- `HomeController.cs`
- `StudentsController.cs`
- `CoursesController.cs` - Also updated file upload handling with IFormFile/IWebHostEnvironment
- `DepartmentsController.cs`
- `InstructorsController.cs` - Edit action made async with TryUpdateModelAsync
- `NotificationsController.cs`
