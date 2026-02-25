# .NET Upgrade Plan: ContosoUniversity .NET Framework 4.8 → .NET 9.0

## Executive Summary

This plan outlines the upgrade of the ContosoUniversity ASP.NET MVC 5 application from .NET Framework 4.8 to .NET 9.0 (Standard Term Support). The application is a web-based university management system built with ASP.NET MVC 5, Entity Framework Core 3.1, and a SQL Server database.

**Project Overview:**
- **Project Name:** ContosoUniversity
- **Current Framework:** .NET Framework 4.8
- **Target Framework:** .NET 9.0 (STS)
- **Project Type:** ASP.NET MVC 5 Web Application
- **Number of Projects:** 1
- **Current Package Count:** 47 NuGet packages

**Upgrade Strategy:** All-At-Once — Single project, cohesive codebase, straightforward migration path from .NET Framework to modern .NET.

**Key Challenges:**
1. **Legacy Project Format:** Project uses non-SDK-style format and must be converted
2. **ASP.NET MVC 5 → ASP.NET Core MVC:** Significant framework shift requiring code changes
3. **Entity Framework Core 3.1:** Needs upgrade to EF Core 9.0
4. **Web.config → appsettings.json:** Configuration system migration
5. **Global.asax → Program.cs/Startup.cs:** Application startup changes
6. **Package Updates:** 47 packages need evaluation and potential upgrades
7. **API Compatibility:** Breaking changes in framework APIs

## Upgrade Philosophy

This upgrade is designed to be executed **automatically and completely without stopping for user confirmation**. Each task contains detailed, actionable instructions that an executing agent can follow independently. The plan prioritizes:

- **Incremental validation:** Build and test after each major change
- **Clear rollback points:** Each task is atomic with well-defined success criteria
- **Dependency ordering:** Tasks execute in correct sequence to avoid breaking dependencies
- **Comprehensive testing:** Verification at every step to catch issues early

## Task Execution Order

The upgrade follows a phased approach with clear dependencies:

### Phase 1: Project Structure Modernization
**Goal:** Convert to SDK-style project format without changing target framework

#### TASK-001: Convert to SDK-style project format
**Type:** Structural conversion  
**Dependencies:** None  
**Risk:** Medium  
**Estimated Impact:** Complete project file restructure

**Actions:**
1. Backup current `ContosoUniversity.csproj` and `packages.config`
2. Convert project file from legacy format to SDK-style using sdk-style-conversion skill
3. Migrate package references from `packages.config` to `<PackageReference>` elements
4. Remove obsolete elements (e.g., `<ProjectTypeGuids>`, `<Import>` statements)
5. Keep target framework as `net48` initially
6. Preserve all existing package versions
7. Remove `packages.config` after successful conversion
8. Verify project still builds: `dotnet build`

**Success Criteria:**
- Project file uses SDK-style format (`<Project Sdk="...">`
- All package references migrated to `<PackageReference>`
- Target framework remains `net48`
- `dotnet build` succeeds with 0 errors
- `packages.config` file removed

---

### Phase 2: Framework and Package Upgrades
**Goal:** Update to .NET 9.0 and upgrade all packages to compatible versions

#### TASK-002: Update target framework and SDK
**Type:** Framework upgrade  
**Dependencies:** TASK-001  
**Risk:** High  
**Estimated Impact:** Framework-wide compatibility issues

**Actions:**
1. Change `<TargetFramework>` from `net48` to `net9.0` in `.csproj`
2. Change `<Project Sdk="Microsoft.NET.Sdk">` to `<Project Sdk="Microsoft.NET.Sdk.Web">`
3. Add `<OutputType>Exe</OutputType>` if not present
4. Remove `<ProjectTypeGuids>` if still present
5. Do NOT build yet - proceed to package updates first

**Success Criteria:**
- Target framework is `net9.0`
- Project SDK is `Microsoft.NET.Sdk.Web`
- Project file is valid (but may not build until packages are updated)

#### TASK-003: Upgrade Entity Framework Core packages
**Type:** Package upgrade  
**Dependencies:** TASK-002  
**Risk:** High  
**Estimated Impact:** Data access layer, database context, migrations

**Actions:**
1. Remove EF Core 3.1 packages:
   - `Microsoft.EntityFrameworkCore` (3.1.32)
   - `Microsoft.EntityFrameworkCore.Abstractions` (3.1.32)
   - `Microsoft.EntityFrameworkCore.Analyzers` (3.1.32)
   - `Microsoft.EntityFrameworkCore.Relational` (3.1.32)
   - `Microsoft.EntityFrameworkCore.SqlServer` (3.1.32)
   - `Microsoft.EntityFrameworkCore.Tools` (3.1.32)

2. Add EF Core 9.0 packages:
   ```xml
   <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
   <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
   <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
   ```

3. Note: `Abstractions`, `Analyzers`, and `Relational` are included transitively in EF Core 9.0

**Success Criteria:**
- All EF Core packages are version 9.0.0
- No EF Core 3.1 packages remain
- Package references are clean (no transitive packages explicitly listed)

#### TASK-004: Upgrade Microsoft.Data.SqlClient
**Type:** Package upgrade (security)  
**Dependencies:** TASK-002  
**Risk:** Medium  
**Estimated Impact:** Database connection strings, SQL Server access

**Actions:**
1. Remove `Microsoft.Data.SqlClient` version 2.1.4 (has security vulnerabilities)
2. Remove `Microsoft.Data.SqlClient.SNI.runtime` version 2.1.1 (transitive, included in newer versions)
3. Add `Microsoft.Data.SqlClient` version 6.0.0 or higher (compatible with .NET 9.0):
   ```xml
   <PackageReference Include="Microsoft.Data.SqlClient" Version="6.0.0" />
   ```

4. Update connection strings if needed (newer versions use `Encrypt=True` by default)

**Success Criteria:**
- `Microsoft.Data.SqlClient` is version 6.0.0 or higher
- No security vulnerabilities reported for SqlClient
- SNI runtime is managed transitively

#### TASK-005: Remove .NET Framework-specific packages
**Type:** Package removal  
**Dependencies:** TASK-002  
**Risk:** Low  
**Estimated Impact:** Reduced package count, cleaner dependencies

**Actions:**
Remove packages that are included in .NET 9.0 or not needed:

1. **Included in .NET 9.0:**
   - `Microsoft.Bcl.AsyncInterfaces` (1.1.1)
   - `Microsoft.Bcl.HashCode` (1.1.1)
   - `NETStandard.Library` (2.0.3)
   - `System.Buffers` (4.5.1)
   - `System.Collections.Immutable` (1.7.1)
   - `System.ComponentModel.Annotations` (4.7.0)
   - `System.Diagnostics.DiagnosticSource` (4.7.1)
   - `System.Memory` (4.5.4)
   - `System.Numerics.Vectors` (4.5.0)
   - `System.Runtime.CompilerServices.Unsafe` (4.5.3)
   - `System.Threading.Tasks.Extensions` (4.5.4)

2. **Not compatible with .NET 9.0 / ASP.NET Core:**
   - `Microsoft.CodeDom.Providers.DotNetCompilerPlatform` (2.0.1) - Roslyn, not needed
   - `Microsoft.Web.Infrastructure` (2.0.1) - ASP.NET Framework only
   - `Antlr` (3.4.1.9004) - Used by Web Optimization, which is replaced
   - `WebGrease` (1.5.2) - Used by Web Optimization
   - `Microsoft.AspNet.Web.Optimization` (1.1.3) - Replaced by built-in bundling/minification

**Success Criteria:**
- All listed packages are removed from `.csproj`
- No build errors from missing types (they're in the framework or not needed)
- Package count reduced by ~16 packages

#### TASK-006: Upgrade Microsoft.Extensions packages
**Type:** Package upgrade  
**Dependencies:** TASK-002  
**Risk:** Medium  
**Estimated Impact:** Dependency injection, configuration, logging

**Actions:**
1. Remove Microsoft.Extensions 3.1.32 packages:
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

2. **Note:** These packages are typically included transitively via ASP.NET Core framework references in .NET 9.0. Only add explicit references if specific features are needed that aren't in the framework.

3. If explicit references are needed, use version 9.0.0:
   ```xml
   <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.0" />
   <!-- Only add others if specifically required -->
   ```

**Success Criteria:**
- Microsoft.Extensions 3.1 packages removed
- Required functionality available through framework references
- No missing dependency errors

#### TASK-007: Upgrade Newtonsoft.Json
**Type:** Package upgrade  
**Dependencies:** TASK-002  
**Risk:** Low  
**Estimated Impact:** JSON serialization

**Actions:**
1. Update `Newtonsoft.Json` from 13.0.3 to latest stable (14.0.0 or higher):
   ```xml
   <PackageReference Include="Newtonsoft.Json" Version="14.0.0" />
   ```

2. Consider migrating to `System.Text.Json` (included in .NET 9.0) for better performance
3. If keeping Newtonsoft.Json, verify JSON serialization settings still work

**Success Criteria:**
- Newtonsoft.Json is version 14.0.0 or higher, or removed if migrated to System.Text.Json
- JSON serialization works correctly

#### TASK-008: Remove or replace ASP.NET MVC 5 packages
**Type:** Package replacement  
**Dependencies:** TASK-002  
**Risk:** High  
**Estimated Impact:** Entire MVC framework - requires code changes

**Actions:**
1. Remove ASP.NET MVC 5 packages (not compatible with .NET 9.0):
   - `Microsoft.AspNet.Mvc` (5.2.9)
   - `Microsoft.AspNet.Razor` (3.2.9)
   - `Microsoft.AspNet.WebPages` (3.2.9)

2. **Note:** These are replaced by ASP.NET Core MVC framework references included in `Microsoft.NET.Sdk.Web`

3. No explicit package addition needed - ASP.NET Core MVC is included in the SDK

**Success Criteria:**
- All ASP.NET MVC 5 packages removed
- ASP.NET Core MVC framework available through SDK

#### TASK-009: Handle jQuery and client-side packages
**Type:** Package evaluation  
**Dependencies:** TASK-002  
**Risk:** Low  
**Estimated Impact:** Client-side scripts and validation

**Actions:**
1. Move client-side packages from NuGet to npm/libman or wwwroot:
   - `jQuery` (3.7.1)
   - `jQuery.Validation` (1.21.0)
   - `Microsoft.jQuery.Unobtrusive.Validation` (4.0.0)
   - `bootstrap` (5.3.3)
   - `Modernizr` (2.6.2)

2. Options:
   - **Option A:** Keep as NuGet packages if they still work
   - **Option B:** Migrate to npm and use LibMan to copy to wwwroot
   - **Option C:** Download and place directly in wwwroot/lib

3. Recommended: Option B (npm + LibMan) for better dependency management

4. Update script references in `_Layout.cshtml` to new paths

**Success Criteria:**
- Client-side libraries available in wwwroot or via npm
- Scripts load correctly in browser
- jQuery validation works

#### TASK-010: Update Microsoft.Identity.Client (if used)
**Type:** Package upgrade  
**Dependencies:** TASK-002  
**Risk:** Low  
**Estimated Impact:** Authentication (if used)

**Actions:**
1. Update `Microsoft.Identity.Client` from 4.21.1 to latest (4.65.0 or higher):
   ```xml
   <PackageReference Include="Microsoft.Identity.Client" Version="4.65.0" />
   ```

2. If not actively used for authentication, consider removing

**Success Criteria:**
- Package is latest version or removed if not used
- Authentication still works if applicable

#### TASK-011: Restore and build after package updates
**Type:** Validation  
**Dependencies:** TASK-003, TASK-004, TASK-005, TASK-006, TASK-007, TASK-008, TASK-009, TASK-010  
**Risk:** High  
**Estimated Impact:** Identifies all package-related issues

**Actions:**
1. Run `dotnet restore` to restore all packages
2. Run `dotnet build` to identify compilation errors
3. **Expected:** Build will fail with errors due to code incompatibilities
4. Document all build errors for next phase
5. Categorize errors:
   - Namespace changes
   - API changes
   - Type removals
   - Syntax incompatibilities

**Success Criteria:**
- `dotnet restore` completes successfully
- All packages compatible with .NET 9.0
- Build errors are catalogued (build failure is expected at this stage)

---

### Phase 3: Code Migration and Compatibility Fixes
**Goal:** Fix all compilation errors and migrate code to ASP.NET Core patterns

#### TASK-012: Migrate application startup from Global.asax to Program.cs
**Type:** Code migration  
**Dependencies:** TASK-011  
**Risk:** High  
**Estimated Impact:** Application initialization, routing, middleware

**Actions:**
1. Create new `Program.cs` file in project root:
   ```csharp
   var builder = WebApplication.CreateBuilder(args);
   
   // Add services to the container
   builder.Services.AddControllersWithViews();
   
   // Add DbContext (migrate from Global.asax or Web.config)
   builder.Services.AddDbContext<SchoolContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
   
   var app = builder.Build();
   
   // Configure the HTTP request pipeline
   if (!app.Environment.IsDevelopment())
   {
       app.UseExceptionHandler("/Home/Error");
       app.UseHsts();
   }
   
   app.UseHttpsRedirection();
   app.UseStaticFiles();
   app.UseRouting();
   app.UseAuthorization();
   
   app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}/{id?}");
   
   app.Run();
   ```

2. Migrate configuration from `Global.asax.cs`:
   - Application_Start → builder.Services configuration
   - Route registration → app.MapControllerRoute
   - Filter registration → builder.Services.AddControllersWithViews()
   - Bundle configuration → app.UseStaticFiles() (or new bundling approach)

3. Delete `Global.asax` and `Global.asax.cs` files

4. If using `App_Start` folder (e.g., `RouteConfig.cs`, `BundleConfig.cs`, `FilterConfig.cs`):
   - Migrate route config to Program.cs
   - Migrate filter config to Program.cs services
   - Replace bundling with ASP.NET Core approach or third-party library
   - Delete `App_Start` folder

**Success Criteria:**
- `Program.cs` created with proper ASP.NET Core initialization
- All startup logic migrated from Global.asax
- `Global.asax` and `App_Start` folder removed
- Application starts without errors (may still have other build errors)

#### TASK-013: Migrate configuration from Web.config to appsettings.json
**Type:** Configuration migration  
**Dependencies:** TASK-011  
**Risk:** Medium  
**Estimated Impact:** Configuration system, connection strings, app settings

**Actions:**
1. Create `appsettings.json` in project root:
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
       "DefaultConnection": "<migrate-from-Web.config>"
     }
   }
   ```

2. Create `appsettings.Development.json` for development settings

3. Migrate settings from `Web.config`:
   - `<connectionStrings>` → `"ConnectionStrings"` section
   - `<appSettings>` → root-level keys or custom sections
   - Environment-specific settings → `appsettings.{Environment}.json`

4. Update connection string format if needed (e.g., `Encrypt=True` for SQL Server)

5. Keep `Web.config` minimal with only required elements (e.g., ASP.NET Core Module for IIS)

6. Update code to use `IConfiguration` instead of `ConfigurationManager`:
   ```csharp
   // Old: ConfigurationManager.ConnectionStrings["DefaultConnection"]
   // New: _configuration.GetConnectionString("DefaultConnection")
   ```

**Success Criteria:**
- `appsettings.json` created with all necessary configuration
- Connection strings migrated and functional
- Configuration accessed via `IConfiguration` interface
- `Web.config` reduced to minimum required elements

#### TASK-014: Migrate DbContext registration to dependency injection
**Type:** Code migration  
**Dependencies:** TASK-012, TASK-013  
**Risk:** Medium  
**Estimated Impact:** Database context usage throughout application

**Actions:**
1. Register DbContext in `Program.cs` services (done in TASK-012):
   ```csharp
   builder.Services.AddDbContext<SchoolContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
   ```

2. Update controllers to accept DbContext via constructor injection:
   ```csharp
   public class StudentController : Controller
   {
       private readonly SchoolContext _context;
       
       public StudentController(SchoolContext context)
       {
           _context = context;
       }
       
       // Use _context instead of creating new instances
   }
   ```

3. Remove manual DbContext instantiation (e.g., `using (var context = new SchoolContext())`)

4. Ensure DbContext is properly disposed (handled automatically by DI container)

5. If using EF Core migrations, verify they still work:
   ```bash
   dotnet ef migrations list
   ```

**Success Criteria:**
- DbContext registered in DI container
- All controllers use injected DbContext
- No manual DbContext creation remains
- Database access works correctly
- EF Core migrations functional

#### TASK-015: Update controller and action attributes
**Type:** Code updates  
**Dependencies:** TASK-012  
**Risk:** Low  
**Estimated Impact:** Controller actions, routing, filters

**Actions:**
1. Update controller base class (if needed):
   - ASP.NET Core uses `Microsoft.AspNetCore.Mvc.Controller`
   - Should work automatically with `using Microsoft.AspNetCore.Mvc;`

2. Update attribute routing (if used):
   - `[Route("api/[controller]")]` remains same
   - `[HttpGet]`, `[HttpPost]`, etc. remain same

3. Update filter attributes:
   - `[Authorize]` → Same namespace change: `Microsoft.AspNetCore.Authorization`
   - `[ValidateAntiForgeryToken]` → Same, namespace: `Microsoft.AspNetCore.Mvc`

4. Update result types if needed:
   - `ActionResult` → Can use more specific types like `IActionResult`
   - Consider using `ActionResult<T>` for API endpoints

**Success Criteria:**
- All controllers inherit from correct base class
- Routing attributes work correctly
- Filter attributes apply properly
- No attribute-related compilation errors

#### TASK-016: Update views and Razor syntax
**Type:** View migration  
**Dependencies:** TASK-012  
**Risk:** Medium  
**Estimated Impact:** All Razor views

**Actions:**
1. Update `_ViewImports.cshtml`:
   ```cshtml
   @using ContosoUniversity
   @using ContosoUniversity.Models
   @using Microsoft.AspNetCore.Mvc.Rendering
   @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
   ```

2. Update `_ViewStart.cshtml` (usually no changes needed):
   ```cshtml
   @{
       Layout = "_Layout";
   }
   ```

3. Update `_Layout.cshtml`:
   - Change `@Scripts.Render()` and `@Styles.Render()` to `<script>` and `<link>` tags
   - Update bundle paths to wwwroot paths
   - Update menu links if needed

4. Update views to use Tag Helpers instead of HTML Helpers:
   ```cshtml
   <!-- Old: @Html.ActionLink("Edit", "Edit", new { id = item.Id }) -->
   <a asp-action="Edit" asp-route-id="@item.Id">Edit</a>
   
   <!-- Old: @Html.BeginForm() -->
   <form asp-action="Create" method="post">
   
   <!-- Old: @Html.ValidationSummary() -->
   <div asp-validation-summary="All"></div>
   
   <!-- Old: @Html.LabelFor(m => m.Name) -->
   <label asp-for="Name"></label>
   
   <!-- Old: @Html.TextBoxFor(m => m.Name) -->
   <input asp-for="Name" class="form-control" />
   
   <!-- Old: @Html.ValidationMessageFor(m => m.Name) -->
   <span asp-validation-for="Name"></span>
   ```

5. Update view compilation:
   - Add `<AddRazorSupportForMvc>true</AddRazorSupportForMvc>` to `.csproj` if needed

**Success Criteria:**
- All views use ASP.NET Core Razor syntax
- Tag Helpers work correctly
- No Razor compilation errors
- Views render properly in browser

#### TASK-017: Update model binding and validation
**Type:** Code updates  
**Dependencies:** TASK-012  
**Risk:** Low  
**Estimated Impact:** Model classes, validation attributes

**Actions:**
1. Verify data annotation attributes (usually compatible):
   - `[Required]`
   - `[StringLength]`
   - `[Range]`
   - `[RegularExpression]`
   - `[Display]`

2. Update namespace if needed:
   ```csharp
   using System.ComponentModel.DataAnnotations;
   ```

3. Add model validation in controllers:
   ```csharp
   if (!ModelState.IsValid)
   {
       return View(model);
   }
   ```

4. Ensure client-side validation works (jQuery Validation + Unobtrusive Validation)

**Success Criteria:**
- All validation attributes work correctly
- Server-side validation functions
- Client-side validation functions
- Error messages display properly

#### TASK-018: Update static file handling and wwwroot
**Type:** Structural change  
**Dependencies:** TASK-012  
**Risk:** Low  
**Estimated Impact:** CSS, JavaScript, images, fonts

**Actions:**
1. Create `wwwroot` folder in project root if it doesn't exist

2. Move static content from project root to wwwroot:
   - `Content/` → `wwwroot/css/`
   - `Scripts/` → `wwwroot/js/`
   - `fonts/` → `wwwroot/fonts/`
   - Images → `wwwroot/images/`

3. Update references in `_Layout.cshtml` and other views:
   ```cshtml
   <!-- Old: @Styles.Render("~/Content/css") -->
   <link rel="stylesheet" href="~/css/site.css" />
   
   <!-- Old: @Scripts.Render("~/bundles/jquery") -->
   <script src="~/lib/jquery/dist/jquery.min.js"></script>
   ```

4. Add `app.UseStaticFiles();` to Program.cs (done in TASK-012)

5. Verify all static files are accessible

**Success Criteria:**
- `wwwroot` folder structure created
- All static files moved to wwwroot
- Static files accessible via browser
- No 404 errors for CSS/JS/images

#### TASK-019: Handle authentication and authorization (if applicable)
**Type:** Code migration  
**Dependencies:** TASK-012  
**Risk:** High  
**Estimated Impact:** User authentication, authorization policies

**Actions:**
1. If using Forms Authentication:
   - Migrate to ASP.NET Core Identity or Cookie Authentication
   - Update `Program.cs`:
     ```csharp
     builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
         .AddCookie();
     
     // Before app.UseAuthorization():
     app.UseAuthentication();
     app.UseAuthorization();
     ```

2. If using Windows Authentication:
   - Enable Windows Authentication in `Program.cs`:
     ```csharp
     builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);
     ```
   - Update `launchSettings.json`:
     ```json
     "iisSettings": {
       "windowsAuthentication": true,
       "anonymousAuthentication": false
     }
     ```

3. Update authorization attributes:
   - `[Authorize]` → Same, but namespace is `Microsoft.AspNetCore.Authorization`
   - `[AllowAnonymous]` → Same

4. Migrate any custom authorization logic to ASP.NET Core policies

**Success Criteria:**
- Authentication system migrated to ASP.NET Core
- Authorization attributes work correctly
- Users can log in/out
- Protected pages enforce authentication

#### TASK-020: Fix remaining compilation errors
**Type:** Code fixes  
**Dependencies:** TASK-012 through TASK-019  
**Risk:** Variable (depends on errors)  
**Estimated Impact:** Various code locations

**Actions:**
1. Run `dotnet build` and catalog remaining errors

2. Common error categories and fixes:
   
   **Namespace changes:**
   - `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`
   - `System.Web.Http` → `Microsoft.AspNetCore.Mvc` (for APIs)
   - `System.Web.Routing` → `Microsoft.AspNetCore.Routing`
   
   **Type renames:**
   - `HttpContext.Current` → `HttpContext` (injected)
   - `Server.MapPath()` → `IWebHostEnvironment.WebRootPath` or `ContentRootPath`
   - `Request.Url` → `HttpContext.Request.GetDisplayUrl()`
   - `Request.QueryString` → `HttpContext.Request.Query`
   
   **Removed APIs:**
   - `HttpUtility.HtmlEncode()` → `HtmlEncoder.Default.Encode()`
   - `HttpServerUtility` → `IWebHostEnvironment`
   - `HttpContext.Cache` → `IMemoryCache` (injected)

3. For each error:
   - Identify the root cause
   - Apply appropriate fix
   - Test the fix builds

4. Use skills if available:
   - `system-security-cryptography-migration` for crypto namespace changes
   - Other migration skills as appropriate

**Success Criteria:**
- `dotnet build` completes with 0 errors
- All compiler warnings reviewed and addressed (or documented as acceptable)
- Application code fully compatible with .NET 9.0

---

### Phase 4: Testing and Validation
**Goal:** Ensure the application functions correctly on .NET 9.0

#### TASK-021: Restore and build the solution
**Type:** Validation  
**Dependencies:** TASK-020  
**Risk:** Low  
**Estimated Impact:** Full solution verification

**Actions:**
1. Clean previous build artifacts:
   ```bash
   dotnet clean
   ```

2. Restore packages:
   ```bash
   dotnet restore
   ```

3. Build solution:
   ```bash
   dotnet build --configuration Release
   ```

4. Verify:
   - 0 errors
   - Review any warnings
   - Check output directory for compiled assemblies

**Success Criteria:**
- `dotnet restore` succeeds
- `dotnet build` succeeds with 0 errors
- Output assemblies target .NET 9.0
- No critical warnings

#### TASK-022: Run unit tests (if they exist)
**Type:** Testing  
**Dependencies:** TASK-021  
**Risk:** Medium  
**Estimated Impact:** Test coverage validation

**Actions:**
1. Identify test projects in solution (if any)

2. Run tests:
   ```bash
   dotnet test --configuration Release
   ```

3. If tests fail:
   - Update test framework packages (e.g., xUnit, NUnit, MSTest)
   - Fix test code for .NET 9.0 compatibility
   - Update mocks/stubs if needed

4. If no tests exist:
   - Document this
   - Consider adding basic smoke tests

**Success Criteria:**
- All existing tests pass
- Test coverage maintained or improved
- No test framework compatibility issues

#### TASK-023: Manual functional testing
**Type:** Testing  
**Dependencies:** TASK-021  
**Risk:** High  
**Estimated Impact:** End-to-end functionality

**Actions:**
1. Run the application:
   ```bash
   dotnet run
   ```

2. Test core user flows:
   - Home page loads
   - Navigation works
   - Student CRUD operations (Create, Read, Update, Delete)
   - Course management
   - Enrollment functionality
   - Database connectivity
   - Authentication/Authorization (if applicable)
   - File uploads (if applicable)
   - Search/filtering
   - Pagination

3. Test in different browsers (if web UI):
   - Chrome
   - Edge
   - Firefox

4. Check browser console for JavaScript errors

5. Review application logs for errors/warnings

6. Document any issues found

**Success Criteria:**
- Application starts without errors
- All major features work correctly
- No runtime exceptions
- No browser console errors
- Database operations succeed

#### TASK-024: Performance and behavior validation
**Type:** Testing  
**Dependencies:** TASK-023  
**Risk:** Medium  
**Estimated Impact:** Non-functional requirements

**Actions:**
1. Compare performance with .NET Framework version (if metrics available):
   - Page load times
   - Database query performance
   - Memory usage
   - CPU usage

2. Verify behavior hasn't changed:
   - Data validation
   - Business logic
   - Error handling
   - Logging

3. Check for .NET 9.0-specific improvements:
   - Startup time
   - Response times
   - Memory footprint

4. Document any significant changes

**Success Criteria:**
- Performance is equal or better than .NET Framework version
- Application behavior is consistent
- No unexpected side effects from upgrade

---

### Phase 5: Cleanup and Documentation
**Goal:** Finalize the upgrade and document changes

#### TASK-025: Remove obsolete files and configurations
**Type:** Cleanup  
**Dependencies:** TASK-024  
**Risk:** Low  
**Estimated Impact:** Clean repository

**Actions:**
1. Remove obsolete files:
   - `packages.config` (if not already removed)
   - `Global.asax` and `Global.asax.cs`
   - `App_Start` folder
   - Old `Web.config` sections (keep minimal for IIS)
   - Unused `bin` and `obj` folders (will be regenerated)

2. Clean up project file:
   - Remove commented-out package references
   - Remove obsolete properties
   - Organize package references alphabetically or by category

3. Update `.gitignore`:
   - Ensure it covers .NET 9.0 build artifacts
   - Add `wwwroot/lib` if using libman

**Success Criteria:**
- No obsolete files in repository
- Clean `.csproj` file
- Proper `.gitignore` configuration

#### TASK-026: Update documentation
**Type:** Documentation  
**Dependencies:** TASK-025  
**Risk:** Low  
**Estimated Impact:** Developer onboarding and maintenance

**Actions:**
1. Update `README.md`:
   - Change framework version to .NET 9.0
   - Update prerequisites (.NET 9.0 SDK required)
   - Update build instructions if changed
   - Update deployment instructions if changed

2. Update setup guides:
   - `SETUP_TESTING_GUIDE.md`
   - Any other relevant documentation files

3. Document breaking changes:
   - Configuration changes
   - API changes
   - Deployment changes

4. Create upgrade notes document:
   - What was changed
   - Why it was changed
   - How to maintain it going forward

**Success Criteria:**
- All documentation reflects .NET 9.0 upgrade
- Prerequisites are accurate
- Setup instructions are current
- Breaking changes are documented

#### TASK-027: Final verification and commit
**Type:** Source control  
**Dependencies:** TASK-026  
**Risk:** Low  
**Estimated Impact:** Source control history

**Actions:**
1. Review all changes:
   ```bash
   git status
   git diff
   ```

2. Stage all changes:
   ```bash
   git add .
   ```

3. Commit with descriptive message:
   ```bash
   git commit -m "Upgrade ContosoUniversity from .NET Framework 4.8 to .NET 9.0
   
   - Convert project to SDK-style format
   - Update target framework to net9.0
   - Upgrade Entity Framework Core from 3.1 to 9.0
   - Upgrade Microsoft.Data.SqlClient to 6.0.0 (fix security vulnerabilities)
   - Migrate from ASP.NET MVC 5 to ASP.NET Core MVC
   - Migrate Global.asax to Program.cs
   - Migrate Web.config to appsettings.json
   - Update all NuGet packages to .NET 9.0 compatible versions
   - Migrate views to use Tag Helpers
   - Move static files to wwwroot
   - Update documentation
   
   All tests pass. Application functional on .NET 9.0."
   ```

4. Tag the commit:
   ```bash
   git tag -a v2.0-net9.0 -m "Upgraded to .NET 9.0"
   ```

5. Push to remote repository:
   ```bash
   git push origin <branch-name>
   git push origin --tags
   ```

**Success Criteria:**
- All changes committed
- Commit message is descriptive
- Version tagged appropriately
- Changes pushed to remote

---

## Package Update Reference

### Packages to Remove (included in .NET 9.0)
| Package | Version | Reason |
|---------|---------|--------|
| `Microsoft.Bcl.AsyncInterfaces` | 1.1.1 | Included in .NET 9.0 |
| `Microsoft.Bcl.HashCode` | 1.1.1 | Included in .NET 9.0 |
| `NETStandard.Library` | 2.0.3 | Not needed for .NET 9.0 |
| `System.Buffers` | 4.5.1 | Included in .NET 9.0 |
| `System.Collections.Immutable` | 1.7.1 | Included in .NET 9.0 |
| `System.ComponentModel.Annotations` | 4.7.0 | Included in .NET 9.0 |
| `System.Diagnostics.DiagnosticSource` | 4.7.1 | Included in .NET 9.0 |
| `System.Memory` | 4.5.4 | Included in .NET 9.0 |
| `System.Numerics.Vectors` | 4.5.0 | Included in .NET 9.0 |
| `System.Runtime.CompilerServices.Unsafe` | 4.5.3 | Included in .NET 9.0 |
| `System.Threading.Tasks.Extensions` | 4.5.4 | Included in .NET 9.0 |

### Packages to Remove (incompatible or obsolete)
| Package | Version | Reason |
|---------|---------|--------|
| `Microsoft.CodeDom.Providers.DotNetCompilerPlatform` | 2.0.1 | Not needed (Roslyn integrated) |
| `Microsoft.Web.Infrastructure` | 2.0.1 | ASP.NET Framework only |
| `Antlr` | 3.4.1.9004 | Used by Web Optimization |
| `WebGrease` | 1.5.2 | Used by Web Optimization |
| `Microsoft.AspNet.Web.Optimization` | 1.1.3 | Replaced by built-in bundling |
| `Microsoft.AspNet.Mvc` | 5.2.9 | Replaced by ASP.NET Core MVC |
| `Microsoft.AspNet.Razor` | 3.2.9 | Replaced by ASP.NET Core Razor |
| `Microsoft.AspNet.WebPages` | 3.2.9 | Replaced by ASP.NET Core |

### Packages to Upgrade
| Package | Current | Target | Notes |
|---------|---------|--------|-------|
| `Microsoft.EntityFrameworkCore` | 3.1.32 | 9.0.0 | Breaking changes expected |
| `Microsoft.EntityFrameworkCore.SqlServer` | 3.1.32 | 9.0.0 | Use with EF Core 9.0 |
| `Microsoft.EntityFrameworkCore.Tools` | 3.1.32 | 9.0.0 | For migrations |
| `Microsoft.Data.SqlClient` | 2.1.4 | 6.0.0+ | Security vulnerability fix |
| `Newtonsoft.Json` | 13.0.3 | 14.0.0+ | Or migrate to System.Text.Json |
| `Microsoft.Identity.Client` | 4.21.1 | 4.65.0+ | If used for auth |

### Packages to Handle Specially (Microsoft.Extensions)
All Microsoft.Extensions 3.1.32 packages are typically included transitively in ASP.NET Core 9.0. Remove explicit references unless specific features are needed:

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

### Client-Side Packages (Move to npm or wwwroot)
| Package | Version | Recommendation |
|---------|---------|----------------|
| `jQuery` | 3.7.1 | Keep latest via npm/libman |
| `jQuery.Validation` | 1.21.0 | Keep via npm/libman |
| `Microsoft.jQuery.Unobtrusive.Validation` | 4.0.0 | Keep via npm/libman |
| `bootstrap` | 5.3.3 | Keep latest via npm/libman |
| `Modernizr` | 2.6.2 | Evaluate if still needed |

---

## Breaking Changes Catalog

### ASP.NET MVC 5 → ASP.NET Core MVC

**Application Startup:**
- `Global.asax` → `Program.cs` with WebApplicationBuilder
- `RouteConfig.cs` → Route mapping in Program.cs
- `FilterConfig.cs` → Services configuration
- `BundleConfig.cs` → Removed (use native bundling or third-party)

**Configuration:**
- `Web.config` → `appsettings.json`
- `ConfigurationManager` → `IConfiguration` (injected)
- `<appSettings>` → JSON configuration sections

**Dependency Injection:**
- No built-in DI → Built-in DI container
- Manual instantiation → Constructor injection

**Controllers:**
- `System.Web.Mvc.Controller` → `Microsoft.AspNetCore.Mvc.Controller`
- `HttpContext.Current` → `HttpContext` (injected or from base class)

**Views:**
- HTML Helpers → Tag Helpers
- `@Html.ActionLink()` → `<a asp-action="">` 
- `@Html.BeginForm()` → `<form asp-action="">`
- `@Html.TextBoxFor()` → `<input asp-for="">`
- Bundle rendering → Direct `<script>` and `<link>` tags

**Static Files:**
- Root-level folders → `wwwroot` folder
- `/Content/` → `/wwwroot/css/`
- `/Scripts/` → `/wwwroot/js/`

**Routing:**
- `MapRoute` in RouteConfig → `MapControllerRoute` in Program.cs
- Attribute routing syntax mostly same

**Authentication:**
- Forms Authentication → Cookie Authentication or ASP.NET Core Identity
- `[Authorize]` namespace changed to `Microsoft.AspNetCore.Authorization`

**HttpContext:**
- `Request.Url` → `Request.GetDisplayUrl()`
- `Request.QueryString` → `Request.Query`
- `Request.Form` → same but accessed differently
- `Server.MapPath()` → `IWebHostEnvironment.ContentRootPath` or `WebRootPath`

### Entity Framework Core 3.1 → 9.0

**Query Changes:**
- Client evaluation disabled by default (was opt-in in 3.1)
- Some LINQ queries may need adjustment

**Migration Changes:**
- Migration command syntax same
- Some migration operations have new APIs

**DbContext Changes:**
- `DbSet` behavior largely same
- Connection string encryption default changed (SQL Server)

**Package Structure:**
- Abstractions and Relational included transitively
- Explicit references not needed

### Microsoft.Data.SqlClient 2.1.4 → 6.0.0

**Breaking Changes:**
- `Encrypt=True` is now default (was False)
- May need to update connection strings: `Encrypt=False;TrustServerCertificate=True` for local dev
- Certificate validation enforced by default

**Security:**
- Fixes multiple vulnerabilities (CVE-YYYY-XXXXX)
- Stronger encryption defaults

---

## Risk Assessment and Mitigation

### High-Risk Areas

#### 1. Application Startup Migration (TASK-012)
**Risk:** Application fails to start due to startup configuration errors  
**Impact:** Complete application failure  
**Mitigation:**
- Follow Program.cs template carefully
- Test application startup after each change
- Keep Global.asax temporarily for reference
- Rollback point: Before TASK-012

#### 2. EF Core 3.1 → 9.0 Upgrade (TASK-003)
**Risk:** Database queries break due to EF Core changes  
**Impact:** Data access failures throughout application  
**Mitigation:**
- Review EF Core 9.0 breaking changes documentation
- Test all database operations after upgrade
- Verify migrations still work
- Have database backup before testing
- Rollback point: After TASK-002

#### 3. ASP.NET MVC 5 → ASP.NET Core MVC (TASK-012, TASK-016)
**Risk:** Views and controllers incompatible with ASP.NET Core  
**Impact:** Pages don't render, actions fail  
**Mitigation:**
- Migrate one controller at a time if needed
- Test each view after migration
- Keep HTML Helpers temporarily if Tag Helpers cause issues
- Rollback point: Before TASK-016

#### 4. Authentication System (TASK-019)
**Risk:** Users cannot log in after migration  
**Impact:** Application inaccessible to authenticated users  
**Mitigation:**
- Document current authentication mechanism thoroughly
- Test authentication immediately after migration
- Have fallback authentication method
- Consider phased rollout
- Rollback point: Before TASK-019

### Medium-Risk Areas

#### 1. Configuration Migration (TASK-013)
**Risk:** Connection strings or app settings incorrect  
**Impact:** Configuration errors, unable to connect to database  
**Mitigation:**
- Validate all configuration values before deleting Web.config sections
- Test with development settings first
- Keep Web.config backup
- Rollback point: Before TASK-013

#### 2. Client-Side Dependencies (TASK-009)
**Risk:** jQuery, Bootstrap, or validation scripts don't load  
**Impact:** Client-side functionality broken  
**Mitigation:**
- Test in browser after moving files
- Check browser console for 404 errors
- Keep NuGet packages temporarily if needed
- Rollback point: Before TASK-009

#### 3. Model Binding Changes (TASK-017)
**Risk:** Form submissions fail due to binding differences  
**Impact:** Unable to submit forms, data loss  
**Mitigation:**
- Test all forms after migration
- Verify validation attributes work
- Check ModelState in controller actions
- Rollback point: Before TASK-017

### Low-Risk Areas

#### 1. Package Removal (TASK-005)
**Risk:** Removed package is actually needed  
**Impact:** Compilation error (easy to fix)  
**Mitigation:**
- Build immediately after removal
- Re-add package if needed
- Rollback point: After TASK-005

#### 2. Documentation Updates (TASK-026)
**Risk:** Minimal - documentation only  
**Impact:** Confusion for developers  
**Mitigation:**
- Review with team
- Update based on feedback

### Rollback Strategy

**Rollback Points:**
1. **After TASK-001:** Can revert to original .csproj and packages.config
2. **After TASK-002:** Can change TargetFramework back to net48
3. **After TASK-011:** Can revert all package changes
4. **After TASK-012:** Can restore Global.asax and remove Program.cs
5. **After TASK-020:** Can revert all code changes
6. **After TASK-027:** Can revert entire Git commit

**Rollback Procedure:**
```bash
# If using Git
git reset --hard HEAD~1  # Revert last commit
git clean -fd           # Remove untracked files

# If manual rollback needed
# Restore backup of .csproj, packages.config, Global.asax, etc.
```

**Backup Strategy:**
- Create Git branch before starting: `git checkout -b upgrade/net9.0`
- Tag before starting: `git tag pre-net9.0-upgrade`
- Keep backups of critical files:
  - `ContosoUniversity.csproj`
  - `packages.config`
  - `Global.asax.cs`
  - `Web.config`

---

## Source Control Strategy

**Branch Strategy:**
- Create feature branch: `upgrade/net9.0-from-net48`
- Make incremental commits after each task or phase
- Merge to main/master only after full validation

**Commit Strategy:**
Prefer single atomic commit for the entire upgrade if possible to maintain clean history:

```bash
# After all tasks complete and validated
git add .
git commit -m "Upgrade to .NET 9.0 from .NET Framework 4.8"
git push origin upgrade/net9.0-from-net48
```

If issues occur, individual commits per task are acceptable for debugging:
- TASK-001: Convert to SDK-style
- TASK-002-011: Update packages
- TASK-012-020: Migrate code
- TASK-021-024: Testing and validation
- TASK-025-027: Cleanup and documentation

**Pull Request Strategy:**
- Create PR from upgrade branch to main
- Include this plan document in PR description
- Require code review before merge
- Run CI/CD pipeline on PR branch before merge
- Verify all tests pass in PR build

---

## Success Criteria

The upgrade is complete and successful when ALL of the following criteria are met:

### Build and Compilation
- [ ] Project targets `net9.0` framework
- [ ] Project uses SDK-style format (`<Project Sdk="Microsoft.NET.Sdk.Web">`)
- [ ] `dotnet restore` completes without errors
- [ ] `dotnet build` completes with 0 errors
- [ ] Build warnings reviewed and addressed (or documented as acceptable)
- [ ] Solution builds in both Debug and Release configurations

### Package Management
- [ ] All packages compatible with .NET 9.0
- [ ] No packages with known security vulnerabilities
- [ ] `packages.config` removed
- [ ] All package references in .csproj file
- [ ] No unnecessary transitive package references
- [ ] Microsoft.Data.SqlClient upgraded to 6.0.0 or higher

### Code Quality
- [ ] No compilation errors
- [ ] All namespaces updated to .NET 9.0 equivalents
- [ ] No obsolete API usage
- [ ] Code follows ASP.NET Core patterns and best practices
- [ ] Dependency injection used for DbContext and services

### Functionality
- [ ] Application starts without errors (`dotnet run` succeeds)
- [ ] Home page loads correctly
- [ ] All CRUD operations work (Create, Read, Update, Delete)
- [ ] Database connectivity functional
- [ ] Entity Framework migrations work
- [ ] Static files load (CSS, JavaScript, images)
- [ ] Client-side validation works
- [ ] Server-side validation works
- [ ] Error handling works correctly
- [ ] Authentication/Authorization works (if applicable)
- [ ] All major user workflows functional

### Testing
- [ ] All unit tests pass (if tests exist)
- [ ] Manual functional testing completed and documented
- [ ] No runtime exceptions during testing
- [ ] No browser console errors
- [ ] Performance is acceptable (equal or better than .NET Framework version)

### Configuration
- [ ] Connection strings migrated to appsettings.json
- [ ] All configuration values migrated from Web.config
- [ ] Configuration works in different environments (Dev, Staging, Production)
- [ ] Sensitive data not committed to source control

### Structure and Organization
- [ ] `wwwroot` folder created with proper structure
- [ ] Static files organized in wwwroot
- [ ] `Program.cs` created with proper startup configuration
- [ ] `Global.asax` removed
- [ ] `App_Start` folder removed
- [ ] Obsolete files cleaned up

### Documentation
- [ ] README.md updated with .NET 9.0 prerequisites
- [ ] Setup guides updated
- [ ] Breaking changes documented
- [ ] Upgrade notes created
- [ ] API documentation updated (if exists)

### Source Control
- [ ] All changes committed to Git
- [ ] Commit message is descriptive and complete
- [ ] Branch pushed to remote repository
- [ ] Version tagged appropriately
- [ ] Pull request created (if using PR workflow)

### Security
- [ ] No security vulnerabilities in dependencies
- [ ] Connection strings secured
- [ ] Secrets moved to configuration (not hardcoded)
- [ ] HTTPS enforced (if applicable)
- [ ] Authentication/Authorization migrated correctly

### Deployment Readiness
- [ ] Application can be published: `dotnet publish`
- [ ] Published output is valid
- [ ] Deployment scripts updated (if exist)
- [ ] CI/CD pipeline updated (if exists)
- [ ] Deployment documentation updated

---

## Post-Upgrade Considerations

### Performance Monitoring
After deploying to production:
- Monitor application performance metrics
- Compare with .NET Framework baseline
- Watch for memory leaks or performance degradation
- Monitor error rates and exceptions

### Team Training
- Train development team on ASP.NET Core patterns
- Update coding standards and guidelines
- Share lessons learned from upgrade
- Document common issues and solutions

### Continuous Improvement
- Consider migrating remaining areas:
  - Newtonsoft.Json → System.Text.Json (if applicable)
  - Consider upgrading to .NET 10.0 LTS when stable
  - Evaluate new .NET 9.0 features for adoption
  - Improve test coverage

### Maintenance
- Keep packages updated regularly
- Monitor for .NET 9.0 updates and patches
- Review breaking changes in future .NET versions
- Plan for next major version upgrade

---

## Additional Resources

### Official Documentation
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [.NET 9.0 Breaking Changes](https://docs.microsoft.com/dotnet/core/compatibility/9.0)
- [Migrate from ASP.NET MVC to ASP.NET Core MVC](https://docs.microsoft.com/aspnet/core/migration/mvc)
- [Entity Framework Core 9.0 Breaking Changes](https://docs.microsoft.com/ef/core/what-is-new/ef-core-9.0/breaking-changes)

### Migration Tools
- [.NET Upgrade Assistant](https://dotnet.microsoft.com/platform/upgrade-assistant)
- [try-convert (SDK-style conversion)](https://github.com/dotnet/try-convert)

### Community Resources
- [ASP.NET Core Migration Guide](https://github.com/dotnet/aspnetcore/issues)
- Stack Overflow: [asp.net-core-migration tag](https://stackoverflow.com/questions/tagged/asp.net-core-migration)

---

## Execution Instructions

**This plan is designed to be executed automatically by an agent without requiring user confirmation or interaction.**

### For Executing Agents:
1. Execute tasks in the exact order specified (TASK-001 through TASK-027)
2. Do not skip any tasks unless explicitly marked as optional
3. If a task fails:
   - Log the error with full details
   - Attempt to fix the issue based on the mitigation strategy
   - If unable to fix, document the failure and continue if task is non-blocking
   - If task is blocking, halt execution and report
4. After each phase, verify the success criteria for that phase
5. Validate the final success criteria before considering upgrade complete
6. Do not pause for confirmations - execute the entire plan end-to-end

### Critical Requirements:
- Follow the dependency chain: Never execute a task before its dependencies are complete
- Validate after each phase before proceeding to the next
- Build and test frequently to catch issues early
- Document any deviations from the plan
- Create proper Git commits as specified

---

*This upgrade plan was generated for the ContosoUniversity application upgrade from .NET Framework 4.8 to .NET 9.0. Execute tasks sequentially, validate thoroughly, and test comprehensively at each stage.*
