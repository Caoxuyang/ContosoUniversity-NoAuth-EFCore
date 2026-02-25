# Task 014 - Migrate DbContext Registration to Dependency Injection

## Summary

Migrated `SchoolContext` from manual instantiation via `SchoolContextFactory` to ASP.NET Core dependency injection.

## Changes Made

### Files Modified
- `Program.cs` - Registered `SchoolContext` with DI container:
  ```csharp
  builder.Services.AddDbContext<SchoolContext>(options =>
      options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
  ```
- `Controllers/BaseController.cs` - Updated constructor to accept `SchoolContext context` parameter via DI instead of calling `SchoolContextFactory.Create()`
- `Controllers/HomeController.cs` - Added constructor accepting `SchoolContext`
- `Controllers/StudentsController.cs` - Added constructor accepting `SchoolContext`
- `Controllers/CoursesController.cs` - Added constructor accepting `SchoolContext` and `IWebHostEnvironment`
- `Controllers/DepartmentsController.cs` - Added constructor accepting `SchoolContext`
- `Controllers/InstructorsController.cs` - Added constructor accepting `SchoolContext`
- `Controllers/NotificationsController.cs` - Added constructor accepting `SchoolContext`
- `Data/SchoolContextFactory.cs` - Updated to `IDesignTimeDbContextFactory<SchoolContext>` for EF Core tooling support

## DI Registration Pattern
All controllers now use constructor injection:
```csharp
public StudentsController(SchoolContext context) : base(context) { }
```

The `BaseController` stores the injected context as `protected SchoolContext db`.
The DI container manages the lifetime (scoped per request) and disposal.
