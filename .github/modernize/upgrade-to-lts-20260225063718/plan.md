# Migration Plan: ContosoUniversity .NET Framework 4.8 → .NET 9.0

## Executive Summary

Migrating ContosoUniversity from ASP.NET MVC 5 on .NET Framework 4.8 to ASP.NET Core MVC on .NET 9.0. This is a comprehensive migration that includes:

- **Project**: 1 web application project (ContosoUniversity.csproj)
- **Current Stack**: ASP.NET MVC 5, Entity Framework Core 3.1, .NET Framework 4.8
- **Target Stack**: ASP.NET Core MVC, Entity Framework Core 9.0, .NET 9.0
- **NuGet Packages**: 47 packages total, 15+ packages need upgrade or replacement
- **Key Changes**: SDK-style project conversion, ASP.NET MVC to ASP.NET Core migration, package updates

**Upgrade Strategy:** All-At-Once — Single project, modernize completely in one pass.

## Important Instructions for Execution Agent

**CRITICAL: You MUST complete this upgrade automatically without stopping or asking the user for confirmation at any point.**

When executing this plan:
1. **Proceed through ALL tasks sequentially** from TASK-001 to TASK-005
2. **Do NOT pause or wait for user input** between tasks
3. **Do NOT ask for permission** to proceed with any changes
4. **Accept all defaults** and continue automatically
5. **Complete the entire upgrade** until all tasks are finished
6. Only stop if you encounter an unrecoverable error that prevents continuation

The goal is full automation — the upgrade must run from start to finish without human intervention.

## Current State Analysis

### Project Structure
- **Solution**: ContosoUniversity.sln
- **Project**: ContosoUniversity.csproj (ASP.NET MVC 5 Web Application)
- **Target Framework**: .NET Framework 4.8 (v4.8)
- **Project Format**: Legacy non-SDK-style (ToolsVersion="15.0")
- **Package Management**: packages.config (legacy)

### Key Dependencies
- **ASP.NET MVC 5.2.9** → Will become ASP.NET Core MVC
- **Entity Framework Core 3.1.32** → Upgrade to 9.0.x
- **Microsoft.Data.SqlClient 2.1.4** → Upgrade to 5.2.x or 6.1.x (security)
- **Various Microsoft.Extensions.*** 3.1.32** → Included in .NET 9.0 runtime
- **Bootstrap 5.3.3**, **jQuery 3.7.1** → Keep (frontend assets)

### Architecture
- **Pattern**: MVC (Model-View-Controller)
- **Data Access**: Entity Framework Core with SchoolContext
- **Database**: SQL Server (via Microsoft.Data.SqlClient)
- **Views**: Razor (.cshtml)
- **Controllers**: StudentsController, CoursesController, InstructorsController, DepartmentsController, NotificationsController

## Migration Tasks Overview

This migration requires 5 major tasks in the following order:

1. **TASK-001**: Convert project to SDK-style format
2. **TASK-002**: Migrate ASP.NET MVC 5 to ASP.NET Core MVC
3. **TASK-003**: Update target framework to .NET 9.0
4. **TASK-004**: Upgrade and modernize NuGet packages
5. **TASK-005**: Update code patterns for .NET 9.0 compatibility

---

## TASK-001: Convert to SDK-Style Project Format

### Objective
Convert ContosoUniversity.csproj from legacy non-SDK-style format to modern SDK-style format. This is a prerequisite for .NET Core/.NET 5+ migration.

### Actions Required

1. **Backup current project file**: Save ContosoUniversity.csproj.backup

2. **Convert to SDK-style**:
   - Replace entire project file with SDK-style format
   - Use `<Project Sdk="Microsoft.NET.Sdk.Web">` for web applications
   - Keep `TargetFramework` as `net48` (no framework change yet)
   - Remove all `<Reference Include>` and package references (will use PackageReference)
   - Simplify file inclusions (SDK-style auto-includes .cs, .cshtml files)

3. **Migrate packages.config to PackageReference**:
   - Convert all packages from packages.config to `<PackageReference>` elements
   - Remove packages.config file after conversion
   - Keep existing versions for now (upgrade in TASK-004)

4. **Preserve critical settings**:
   - `<RootNamespace>ContosoUniversity</RootNamespace>`
   - `<AssemblyName>ContosoUniversity</AssemblyName>`
   - `<ProjectGuid>` (for solution compatibility)

5. **Verify build**: Run `dotnet build` to ensure project still builds on .NET Framework 4.8

### Expected Outcome
- ContosoUniversity.csproj is SDK-style format
- Still targets net48
- Project builds successfully
- No runtime changes yet

---

## TASK-002: Migrate ASP.NET MVC 5 to ASP.NET Core MVC

### Objective
Migrate from ASP.NET MVC 5 (System.Web-based) to ASP.NET Core MVC. This is the most complex task involving architectural changes.

### Key Changes Required

#### 1. **Update Project SDK**
Change from `Microsoft.NET.Sdk.Web` to ensure proper ASP.NET Core support.

#### 2. **Remove ASP.NET MVC 5 Packages**
Remove packages that are part of ASP.NET Framework and incompatible with .NET Core:
- `Microsoft.AspNet.Mvc`
- `Microsoft.AspNet.Razor`
- `Microsoft.AspNet.WebPages`
- `Microsoft.AspNet.Web.Optimization`
- `Microsoft.Web.Infrastructure`
- `Microsoft.CodeDom.Providers.DotNetCompilerPlatform`

#### 3. **Add ASP.NET Core Packages**
Add the ASP.NET Core framework:
- ASP.NET Core MVC is included in .NET 9.0 runtime (no explicit package needed)
- Keep existing EF Core packages (upgrade versions in TASK-004)

#### 4. **Code Changes**

**a) Update Global.asax → Program.cs + Startup.cs**

Create **Program.cs** (ASP.NET Core entry point):
```csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```

Create **Startup.cs** (replaces Global.asax logic):
```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();
        
        // Add DbContext
        services.AddDbContext<SchoolContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        
        // Add services
        services.AddScoped<NotificationService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}
```

**b) Update Controllers**

Remove System.Web.Mvc namespaces, use Microsoft.AspNetCore.Mvc:

File: **Controllers/StudentsController.cs**, **CoursesController.cs**, etc.
```csharp
// OLD:
using System.Web.Mvc;

// NEW:
using Microsoft.AspNetCore.Mvc;
```

Update return types:
- `HttpNotFoundResult` → `NotFoundResult`
- `HttpStatusCodeResult` → `StatusCodeResult`
- `ActionResult` → `IActionResult` (preferred)

**c) Update Views Configuration**

Update **Views/Web.config** → Convert to **_ViewImports.cshtml**:

Create **Views/_ViewImports.cshtml**:
```cshtml
@using ContosoUniversity
@using ContosoUniversity.Models
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

Remove Views/Web.config (no longer needed)

**d) Update Configuration**

Create **appsettings.json** (replaces Web.config for app settings):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "[Copy from Web.config]"
  }
}
```

Keep Web.config for IIS-specific settings if deploying to IIS.

**e) Update Data Context**

File: **Data/SchoolContext.cs**

Update to use ASP.NET Core patterns:
```csharp
// Ensure DbContext is configured via DI in Startup.cs
// No changes to SchoolContext class itself needed
```

**f) Update Static Files**

- Move static files (CSS, JS, images) to **wwwroot/** directory
- Update paths in _Layout.cshtml to use `~/` root-relative paths
- ASP.NET Core serves static files from wwwroot by default

#### 5. **Update Dependency Injection**

Replace manual instantiation with constructor injection:
```csharp
// OLD (manual instantiation):
private SchoolContext db = new SchoolContext();

// NEW (constructor injection):
private readonly SchoolContext _context;

public StudentsController(SchoolContext context)
{
    _context = context;
}
```

### Expected Outcome
- Project uses ASP.NET Core MVC architecture
- Still targets net48 temporarily (upgrade in TASK-003)
- Controllers, views, and data access updated for ASP.NET Core patterns
- Application structure follows ASP.NET Core conventions

---

## TASK-003: Update Target Framework to .NET 9.0

### Objective
Change the target framework from .NET Framework 4.8 to .NET 9.0.

### Actions Required

1. **Update TargetFramework**:

Edit **ContosoUniversity.csproj**:
```xml
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
</PropertyGroup>
```

2. **Verify .NET 9.0 SDK is installed**:
```bash
dotnet --list-sdks
```
Ensure .NET 9.0 SDK is available.

3. **Build the project**:
```bash
dotnet build
```

Fix any compilation errors related to API changes (handled in TASK-005).

### Expected Outcome
- Project targets net9.0
- Project builds (may have warnings)

---

## TASK-004: Upgrade and Modernize NuGet Packages

### Objective
Update all NuGet packages to versions compatible with .NET 9.0, remove obsolete packages, and address security vulnerabilities.

### Packages to Remove
These are included in .NET 9.0 runtime or no longer needed:

1. `System.Buffers` — Included in runtime
2. `System.Memory` — Included in runtime
3. `System.Numerics.Vectors` — Included in runtime
4. `System.Runtime.CompilerServices.Unsafe` — Included in runtime
5. `System.Threading.Tasks.Extensions` — Included in runtime
6. `System.Collections.Immutable` — Included in runtime
7. `System.Diagnostics.DiagnosticSource` — Included in runtime
8. `Microsoft.Bcl.AsyncInterfaces` — Included in runtime
9. `Microsoft.Bcl.HashCode` — Included in runtime
10. `NETStandard.Library` — No longer needed
11. `Antlr` — ASP.NET Core doesn't use this
12. `WebGrease` — Bundling/minification handled differently
13. `Modernizr` — Keep as static file, remove package

### Packages to Upgrade

#### Entity Framework Core (3.1.32 → 9.0.x)
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
<!-- These are now included in the main package, remove: -->
<!-- Microsoft.EntityFrameworkCore.Abstractions -->
<!-- Microsoft.EntityFrameworkCore.Relational -->
<!-- Microsoft.EntityFrameworkCore.Analyzers -->
```

#### Microsoft.Data.SqlClient (2.1.4 → 5.2.x) **SECURITY UPDATE**
```xml
<PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.1" />
```
**Note**: Version 2.1.4 has known security vulnerabilities. Upgrade to 5.2.1 or later.

#### Microsoft.Extensions.* (3.1.32 → Included in runtime)
These packages are now part of .NET 9.0 runtime, remove explicit references:
- `Microsoft.Extensions.Caching.Abstractions`
- `Microsoft.Extensions.Caching.Memory`
- `Microsoft.Extensions.Configuration`
- `Microsoft.Extensions.Configuration.Abstractions`
- `Microsoft.Extensions.Configuration.Binder`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.Logging`
- `Microsoft.Extensions.Logging.Abstractions`
- `Microsoft.Extensions.Options`
- `Microsoft.Extensions.Primitives`

#### Other Packages
```xml
<!-- Keep, already compatible -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />

<!-- Identity - Upgrade if using authentication -->
<PackageReference Include="Microsoft.Identity.Client" Version="4.61.0" />

<!-- Frontend packages - Keep as-is, served as static files -->
<!-- bootstrap, jQuery handled via wwwroot -->
```

### Actions Required

1. **Remove obsolete packages** from .csproj
2. **Update Entity Framework Core** packages to 9.0.0
3. **Update Microsoft.Data.SqlClient** to 5.2.1 (security)
4. **Remove Microsoft.Extensions.*** explicit references (included in runtime)
5. **Run package restore**:
   ```bash
   dotnet restore
   ```
6. **Verify no security vulnerabilities**:
   ```bash
   dotnet list package --vulnerable
   ```

### Expected Outcome
- All packages compatible with .NET 9.0
- No security vulnerabilities
- Reduced package count (many now in runtime)

---

## TASK-005: Update Code Patterns for .NET 9.0 Compatibility

### Objective
Fix compilation errors and warnings related to API changes between .NET Framework 4.8 and .NET 9.0.

### Common API Changes to Address

#### 1. **Async Pattern Updates**
- Ensure async methods use `async/await` properly
- Use `IAsyncEnumerable<T>` where appropriate

#### 2. **Nullable Reference Types**
Consider enabling:
```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
</PropertyGroup>
```
Fix null-related warnings if enabled.

#### 3. **EF Core 9.0 Changes**
- Review any breaking changes from EF Core 3.1 to 9.0
- Update query patterns if needed
- Check for obsolete APIs

#### 4. **Controller Action Updates**
Ensure all controller actions return proper types:
```csharp
// Use IActionResult or ActionResult<T>
public IActionResult Index() { ... }
public async Task<IActionResult> Edit(int id) { ... }
```

#### 5. **Configuration Access**
Use `IConfiguration` via dependency injection:
```csharp
private readonly IConfiguration _configuration;

public MyController(IConfiguration configuration)
{
    _configuration = configuration;
}

var connectionString = _configuration.GetConnectionString("DefaultConnection");
```

#### 6. **File Upload Changes**
If handling file uploads, update from `HttpPostedFileBase` to `IFormFile`:
```csharp
// OLD:
public ActionResult Upload(HttpPostedFileBase file) { ... }

// NEW:
public async Task<IActionResult> Upload(IFormFile file) { ... }
```

### Actions Required

1. **Build the project** and review all errors/warnings
2. **Fix compilation errors** related to API changes
3. **Update deprecated patterns** to modern equivalents
4. **Run the application** and test core functionality
5. **Fix runtime errors** if any

### Expected Outcome
- Project builds with 0 errors
- No security vulnerabilities
- Application runs successfully on .NET 9.0

---

## Success Criteria

### Build & Compilation
- [ ] `dotnet build` succeeds with 0 errors
- [ ] Project targets net9.0
- [ ] All projects compile successfully

### Package Health
- [ ] No security vulnerabilities (`dotnet list package --vulnerable` returns clean)
- [ ] All packages compatible with .NET 9.0
- [ ] No deprecated packages remain

### Functionality
- [ ] Application starts successfully
- [ ] Database connection works
- [ ] CRUD operations function correctly
- [ ] Views render properly
- [ ] Static files (CSS, JS) load correctly

### Code Quality
- [ ] No compiler warnings related to obsolete APIs
- [ ] Async patterns properly implemented
- [ ] Dependency injection used throughout

---

## Post-Migration Recommendations

After successful migration to .NET 9.0:

1. **Performance Testing**: Benchmark application performance (should improve)
2. **Security Audit**: Review authentication/authorization patterns
3. **Modernize Frontend**: Consider updating jQuery/Bootstrap to latest versions
4. **Add Tests**: Implement unit and integration tests
5. **Enable Nullable Reference Types**: Improve null safety
6. **CI/CD Pipeline**: Set up automated builds and deployments
7. **Consider Minimal APIs**: For new endpoints, explore minimal API patterns

---

## Rollback Plan

If migration encounters critical issues:

1. **Revert to backup**: Use ContosoUniversity.csproj.backup
2. **Restore packages.config**: Restore from backup
3. **Rebuild**: Run `dotnet build` on original configuration
4. **Document issues**: Note what caused the rollback for investigation

---

## Estimated Effort

- **TASK-001**: 30 minutes (SDK-style conversion)
- **TASK-002**: 2-3 hours (ASP.NET Core migration)
- **TASK-003**: 15 minutes (Framework update)
- **TASK-004**: 45 minutes (Package updates)
- **TASK-005**: 1-2 hours (Code fixes and testing)

**Total**: 4-6 hours for complete migration

---

## References

- [Migrate from ASP.NET MVC to ASP.NET Core MVC](https://learn.microsoft.com/aspnet/core/migration/mvc)
- [Migrate from .NET Framework to .NET](https://learn.microsoft.com/dotnet/core/porting/)
- [What's new in .NET 9](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-9)
- [EF Core 9.0 Breaking Changes](https://learn.microsoft.com/ef/core/what-is-new/ef-core-9.0/breaking-changes)

---

**End of Migration Plan**
