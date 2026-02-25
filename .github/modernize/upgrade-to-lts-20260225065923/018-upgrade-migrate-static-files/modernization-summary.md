# Task 018 - Update Static File Handling and Create wwwroot Structure

## Summary

Created wwwroot folder structure and moved static files for ASP.NET Core static file serving.

## Changes Made

### Files Created
- `wwwroot/css/` directory - CSS files
  - `wwwroot/css/Site.css` - Copied from `Content/Site.css`
  - `wwwroot/css/notifications.css` - Copied from `Content/notifications.css`
- `wwwroot/js/` directory - JavaScript files
  - All JS files copied from `Scripts/` directory
- `wwwroot/Uploads/TeachingMaterials/` - Upload directory for course images

### Files Modified
- `Views/Shared/_Layout.cshtml` - Updated static file references:
  - `@Styles.Render("~/Content/css")` → `<link rel="stylesheet" href="~/css/Site.css" />`
  - `@Scripts.Render("~/bundles/jquery")` → `<script src="~/js/jquery-3.4.1.min.js"></script>`
  - `@Scripts.Render("~/bundles/bootstrap")` → `<script src="~/js/bootstrap.min.js"></script>`
  - `<script src="~/Scripts/notifications.js">` → `<script src="~/js/notifications.js">`
- `Controllers/CoursesController.cs` - Updated file upload paths:
  - `Server.MapPath("~/Uploads/TeachingMaterials/")` → `Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "TeachingMaterials")`
  - `~/Uploads/TeachingMaterials/{fileName}` → `/Uploads/TeachingMaterials/{fileName}`

### Program.cs
Added `app.UseStaticFiles()` middleware to serve files from `wwwroot/`.

## Static Files Path Mapping
| Old Path | New wwwroot Path |
|---|---|
| `~/Content/Site.css` | `~/css/Site.css` |
| `~/Content/notifications.css` | `~/css/notifications.css` |
| `~/Scripts/jquery-3.4.1.min.js` | `~/js/jquery-3.4.1.min.js` |
| `~/Scripts/bootstrap.min.js` | `~/js/bootstrap.min.js` |
| `~/Scripts/notifications.js` | `~/js/notifications.js` |
