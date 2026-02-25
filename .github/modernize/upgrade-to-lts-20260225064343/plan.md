# .NET Framework 4.8 to .NET 9.0 Upgrade Plan

## Executive Summary

This plan outlines the upgrade path for ContosoUniversity from .NET Framework 4.8 to .NET 9.0. The application is an ASP.NET MVC 5 web application with Entity Framework Core 3.1, requiring migration to ASP.NET Core MVC and modern .NET.

### Scope

- **Projects**: 1 project (ContosoUniversity.csproj)
- **Current Framework**: .NET Framework 4.8
- **Target Framework**: .NET 9.0
- **Strategy**: All-At-Once (single atomic operation)
- **Complexity Rating**: High

### Key Challenges

1. **ASP.NET MVC 5 to ASP.NET Core Migration**: Complete application model transformation required
2. **SDK-Style Conversion**: Legacy project format must convert to modern SDK-style
3. **MSMQ Dependency**: System.Messaging not supported in .NET Core/9.0 (requires alternative)
4. **Web.config to appsettings.json**: Configuration system migration required
5. **Global.asax to Program.cs/Startup.cs**: Application startup model change

### Success Criteria

- Project targets .NET 9.0
- Application builds without errors
- All features functional (students, courses, instructors, departments)
- Configuration properly migrated
- MSMQ notification system replaced or adapted

---

## Upgrade Strategy

### Selected Approach: All-At-Once

**Rationale**: Single project solution with limited external dependencies makes coordinated atomic upgrade the most efficient approach. All changes occur simultaneously in one operation.

**Execution Model**: Atomic transformation performed in sequential phases, each phase containing related changes that occur together.

---

## Pre-Upgrade Requirements

### Prerequisites

| Requirement | Status | Action Required |
|-------------|--------|-----------------|
| .NET 9.0 SDK | Must Verify | Run `dotnet --list-sdks` to confirm 9.0.x installed |
| Visual Studio 2022 | Recommended | Version 17.8+ for .NET 9 support |
| EF Core tools | Must Install | `dotnet tool install --global dotnet-ef` |
| Backup | Required | Commit all changes, create backup branch |

### Source Control Setup

Before beginning upgrade:
```bash
# Ensure clean working directory
git status

# Create backup branch
git branch backup/pre-net9-upgrade

# Create upgrade working branch
git checkout -b upgrade/net9-migration

# Commit current state if needed
git add .
git commit -m "Pre-upgrade checkpoint"
```

---

## Phase 1: Project Structure Modernization

### Objective
Convert legacy .NET Framework project to modern SDK-style format compatible with .NET 9.0.

### Tasks

#### 1.1 Backup and Prepare
- Create backup of ContosoUniversity.csproj
- Create backup of packages.config
- Document any custom MSBuild targets or imports

#### 1.2 Convert to SDK-Style Project
**File**: ContosoUniversity.csproj

Replace entire project file content with SDK-style format:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>ContosoUniversity</RootNamespace>
    <AssemblyName>ContosoUniversity</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <!-- Package references will be added in Phase 2 -->
  </ItemGroup>
</Project>
```

**Critical Actions**:
- Remove packages.config file after conversion
- SDK-style projects auto-include *.cs files (explicit <Compile> items not needed)
- Web assets (CSS, JS) under wwwroot (will reorganize in Phase 3)

#### 1.3 Update Solution File
No changes needed - ContosoUniversity.sln will work with SDK-style project.

---

## Phase 2: Package and Dependency Migration

### Objective
Replace .NET Framework-specific packages with .NET 9.0-compatible equivalents.

### Package Migration Matrix

| Current Package | Current Version | Target Package | Target Version | Notes |
|----------------|-----------------|----------------|----------------|-------|
| Microsoft.AspNet.Mvc | 5.2.9 | (Framework) | - | Replaced by ASP.NET Core MVC framework |
| Microsoft.AspNet.Razor | 3.2.9 | (Framework) | - | Included in ASP.NET Core |
| Microsoft.AspNet.WebPages | 3.2.9 | (Framework) | - | Included in ASP.NET Core |
| Microsoft.AspNet.Web.Optimization | 1.1.3 | (Remove) | - | Use built-in bundling/minification |
| Microsoft.EntityFrameworkCore | 3.1.32 | Microsoft.EntityFrameworkCore | 9.0.0 | Upgrade to .NET 9 version |
| Microsoft.EntityFrameworkCore.SqlServer | 3.1.32 | Microsoft.EntityFrameworkCore.SqlServer | 9.0.0 | Upgrade to .NET 9 version |
| Microsoft.EntityFrameworkCore.Tools | 3.1.32 | Microsoft.EntityFrameworkCore.Tools | 9.0.0 | Upgrade to .NET 9 version |
| Newtonsoft.Json | 13.0.3 | (Optional) | 13.0.3 | Consider System.Text.Json (built-in) |
| System.Messaging | (Via Framework) | MSMQ.Messaging | 1.3.0 | MSMQ adapter for .NET Core |
| Microsoft.Data.SqlClient | 2.1.4 | Microsoft.Data.SqlClient | 5.2.0 | Update to latest |
| Microsoft.Identity.Client | 4.21.1 | Microsoft.Identity.Client | 4.66.2 | Update to latest |

### Package Reference Updates

**File**: ContosoUniversity.csproj

Add to `<ItemGroup>`:

```xml
<ItemGroup>
  <!-- ASP.NET Core (no explicit package needed - included in SDK) -->
  
  <!-- Entity Framework Core -->
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>

  <!-- Data Access -->
  <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.0" />

  <!-- JSON Processing (optional - if keeping Newtonsoft.Json) -->
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />

  <!-- MSMQ Support for .NET Core -->
  <PackageReference Include="MSMQ.Messaging" Version="1.3.0" />

  <!-- Identity (if used) -->
  <PackageReference Include="Microsoft.Identity.Client" Version="4.66.2" />
</ItemGroup>
```

**Critical**: After updating packages:
```bash
dotnet restore
```

---

## Phase 3: Application Structure Transformation

### Objective
Transform ASP.NET MVC 5 structure to ASP.NET Core MVC structure.

### 3.1 Create wwwroot Directory

**Actions**:
- Create `wwwroot` folder at project root
- Move Content → wwwroot/css
- Move Scripts → wwwroot/js  
- Move Uploads → wwwroot/uploads
- Move favicon.ico → wwwroot/

**Directory Structure (After)**:
```
ContosoUniversity/
├── wwwroot/
│   ├── css/
│   ├── js/
│   ├── uploads/
│   └── favicon.ico
├── Controllers/
├── Models/
├── Views/
├── Data/
├── Services/
└── ...
```

### 3.2 Replace Global.asax with Program.cs

**Remove**: Global.asax, Global.asax.cs

**Create**: Program.cs

```csharp
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure Entity Framework
builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add notification service
builder.Services.AddSingleton<ContosoUniversity.Services.NotificationService>();

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

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SchoolContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
```

### 3.3 Remove App_Start Folder
**Action**: Delete App_Start folder entirely
- RouteConfig.cs → routing now in Program.cs
- FilterConfig.cs → filters registered in Program.cs services
- BundleConfig.cs → bundling handled differently in ASP.NET Core

### 3.4 Replace Web.config with appsettings.json

**Remove**: Web.config, Web.Debug.config, Web.Release.config

**Create**: appsettings.json

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
    "DefaultConnection": "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=ContosoUniversityNoAuthEFCore;Integrated Security=True;MultipleActiveResultSets=True"
  },
  "NotificationQueuePath": ".\\Private$\\ContosoUniversityNotifications"
}
```

**Create**: appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

---

## Phase 4: Code Migration and Breaking Changes

### Objective
Update code to work with ASP.NET Core APIs and .NET 9.0.

### 4.1 Controller Updates

**All Controllers** (BaseController.cs, HomeController.cs, StudentsController.cs, etc.)

**Changes Required**:
```csharp
// REMOVE (ASP.NET MVC 5):
using System.Web.Mvc;

// ADD (ASP.NET Core):
using Microsoft.AspNetCore.Mvc;

// BaseController.cs - Update OnException override:
// OLD:
protected override void OnException(ExceptionContext filterContext)

// NEW:
public override void OnException(ExceptionContext context)
```

**Key API Changes**:
- `ActionResult` → same in ASP.NET Core
- `Controller` → same base class, different namespace
- `HttpNotFound()` → `NotFound()`
- `HttpStatusCodeResult(500)` → `StatusCode(500)`
- `Request.RequestContext` → `HttpContext.Request`

### 4.2 View Updates

**Views/Web.config** → **Remove** (not needed in ASP.NET Core)

**Create**: Views/_ViewImports.cshtml

```cshtml
@using ContosoUniversity
@using ContosoUniversity.Models
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

**Update All Views**:
- `@Html.AntiForgeryToken()` → works same way
- `@Url.Content("~/...")` → `~/...` (works directly)
- Validation attributes work same way

### 4.3 Data Context Updates

**File**: Data/SchoolContext.cs

```csharp
// REMOVE:
using System.Data.Entity;

// ADD:
using Microsoft.EntityFrameworkCore;

// Constructor change:
// OLD (if exists):
public SchoolContext() : base("name=DefaultConnection")

// NEW:
public SchoolContext(DbContextOptions<SchoolContext> options) 
    : base(options)
{
}

// Keep existing:
public DbSet<Student> Students { get; set; }
public DbSet<Course> Courses { get; set; }
// ... etc
```

**File**: Data/SchoolContextFactory.cs

**Purpose**: Design-time EF Core context factory for migrations.

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ContosoUniversity.Data
{
    public class SchoolContextFactory : IDesignTimeDbContextFactory<SchoolContext>
    {
        public SchoolContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<SchoolContext>();
            optionsBuilder.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"));

            return new SchoolContext(optionsBuilder.Options);
        }
    }
}
```

### 4.4 NotificationService MSMQ Migration

**File**: Services/NotificationService.cs

```csharp
// REMOVE:
using System.Messaging;

// ADD:
using MSMQ.Messaging;

// Code changes:
// MessageQueue API remains similar with MSMQ.Messaging adapter
// Verify queue creation and message sending/receiving logic
// Test thoroughly as MSMQ.Messaging is a community package
```

**Alternative Approach** (Recommended for Production):
Consider migrating from MSMQ to Azure Service Bus or RabbitMQ for better cross-platform support. This would require:
- Different NuGet package (Azure.Messaging.ServiceBus or RabbitMQ.Client)
- Code refactoring in NotificationService
- Infrastructure setup

**For This Upgrade**: Retain MSMQ using MSMQ.Messaging package to minimize scope.

### 4.5 Remove Unsupported APIs

**Search for and Update**:
- `HttpContext.Current` → Inject `IHttpContextAccessor`, use `_httpContextAccessor.HttpContext`
- `Server.MapPath("~/...")` → Use `IWebHostEnvironment.ContentRootPath` or `WebRootPath`
- `ConfigurationManager.AppSettings` → Inject `IConfiguration`, use `_configuration["key"]`
- `ConfigurationManager.ConnectionStrings` → `_configuration.GetConnectionString("name")`

**Dependency Injection Required**:
Add constructor injection to controllers/services that need these:

```csharp
private readonly IWebHostEnvironment _environment;
private readonly IConfiguration _configuration;

public HomeController(IWebHostEnvironment environment, IConfiguration configuration)
{
    _environment = environment;
    _configuration = configuration;
}
```

### 4.6 Model Validation

**Models** (Student.cs, Course.cs, etc.)

**Changes**:
- `System.ComponentModel.DataAnnotations` → same namespace, but ensure using .NET 9 version
- Validation attributes largely unchanged
- Test all validation scenarios

---

## Phase 5: Build and Compilation

### Objective
Resolve all compilation errors and warnings.

### 5.1 Initial Build

```bash
dotnet build
```

**Expected Issues**:
1. Namespace conflicts (System.Web.Mvc vs Microsoft.AspNetCore.Mvc)
2. Missing using directives
3. API signature changes
4. Obsolete API usage

### 5.2 Resolve Compilation Errors

**Common Fixes**:

| Error Type | Solution |
|------------|----------|
| 'Controller' ambiguous reference | Remove `using System.Web.Mvc;` |
| 'HttpNotFound' not found | Change to `NotFound()` |
| 'HttpContext.Current' not found | Inject `IHttpContextAccessor` |
| 'Server.MapPath' not found | Inject `IWebHostEnvironment` |
| 'RouteConfig' not found | Remove references (routing in Program.cs) |

**Iterative Process**:
1. Build
2. Fix top error
3. Rebuild
4. Repeat until clean build

### 5.3 Address Warnings

- Nullable reference warnings (if enabled)
- Obsolete API warnings
- Unused using directives

Target: Zero warnings for clean upgrade.

---

## Phase 6: Testing and Validation

### Objective
Verify all functionality works correctly in .NET 9.0.

### 6.1 Unit Testing (if tests exist)

```bash
dotnet test
```

If no test project exists, proceed to manual testing.

### 6.2 Runtime Testing

**Start Application**:
```bash
dotnet run
```

**Test Scenarios**:

| Feature | Test Actions | Expected Result |
|---------|--------------|-----------------|
| Home Page | Navigate to `/` | Displays student statistics |
| Students List | Navigate to `/Students` | Lists all students with pagination |
| Student Create | Create new student | Student saved to database |
| Student Edit | Edit existing student | Changes persisted |
| Student Delete | Delete student | Student removed |
| Students Search | Search by name | Filtered results display |
| Courses List | Navigate to `/Courses` | Lists all courses |
| Course CRUD | Create/Edit/Delete course | Operations succeed |
| Instructors | View/Edit instructors | Instructor data displays/updates |
| Departments | View/Edit departments | Department data displays/updates |
| Notifications | Trigger notification | MSMQ message sent (verify queue) |
| File Upload | Upload teaching material | File saved to wwwroot/uploads |

### 6.3 Database Verification

```bash
# Ensure EF Core migrations work
dotnet ef migrations list

# Test database connection
dotnet ef database update
```

### 6.4 Configuration Verification

- Connection string works
- appsettings.json values read correctly
- Environment-specific config (Development vs Production)

---

## Phase 7: Finalization

### Objective
Complete the upgrade with final cleanup and documentation.

### 7.1 Cleanup

**Remove Old Files**:
- `obj/` and `bin/` directories (rebuild generates new)
- `packages/` folder (no longer used with SDK-style)
- `.vs/` folder (Visual Studio cache)
- `*.user` files
- Backup files (*.csproj.backup, etc.)

**Verify Project Structure**:
```
ContosoUniversity/
├── wwwroot/
├── Controllers/
├── Models/
├── Views/
├── Data/
├── Services/
├── Properties/
├── ContosoUniversity.csproj
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

### 7.2 Update Documentation

**README.md** (create if missing):
```markdown
# Contoso University

ASP.NET Core MVC application on .NET 9.0

## Prerequisites
- .NET 9.0 SDK
- SQL Server or LocalDB
- MSMQ (for notifications)

## Setup
1. Clone repository
2. Update connection string in appsettings.json
3. Run `dotnet restore`
4. Run `dotnet ef database update`
5. Run `dotnet run`

## Features
- Student management
- Course management
- Instructor management
- Department management
- Notifications via MSMQ
```

### 7.3 Source Control Commit

**Single Commit Approach** (Recommended):

```bash
git add .
git commit -m "Upgrade to .NET 9.0

- Convert to SDK-style project
- Migrate ASP.NET MVC 5 to ASP.NET Core MVC
- Update Entity Framework Core 3.1 to 9.0
- Replace Web.config with appsettings.json
- Replace Global.asax with Program.cs
- Update all NuGet packages to .NET 9-compatible versions
- Adapt MSMQ usage with MSMQ.Messaging adapter
- Reorganize static files to wwwroot structure
- Update controller and view namespaces

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

**Alternative Multi-Commit Approach**:
If tracking granular changes:
- Commit 1: Project file conversion
- Commit 2: Package updates
- Commit 3: Structure changes
- Commit 4: Code updates
- Commit 5: Testing fixes

---

## Risk Assessment

### High-Risk Areas

| Area | Risk Level | Mitigation Strategy |
|------|------------|---------------------|
| MSMQ Compatibility | **High** | Use MSMQ.Messaging adapter; thoroughly test notification system; document fallback to Azure Service Bus if issues arise |
| ASP.NET Core API Differences | **High** | Systematic controller/view updates; comprehensive testing of all endpoints |
| EF Core 3.1 → 9.0 Migration | **Medium** | Review breaking changes documentation; test all queries and migrations |
| Configuration Migration | **Medium** | Careful Web.config → appsettings.json mapping; verify all keys transferred |
| Static File Serving | **Low** | wwwroot structure is standard; verify file paths in views |

### Rollback Plan

If critical issues arise:

```bash
# Revert to backup branch
git checkout backup/pre-net9-upgrade

# Or reset current branch
git reset --hard HEAD~1  # If committed
git checkout .           # If not committed
```

---

## Complexity Assessment

### Overall Complexity: **High**

**Factors Contributing to Complexity**:
1. **Framework Shift**: ASP.NET MVC 5 → ASP.NET Core (major paradigm change)
2. **Project Format**: Legacy → SDK-style (requires complete project file rewrite)
3. **Configuration Model**: Web.config → appsettings.json (different approach)
4. **Application Startup**: Global.asax → Program.cs (new pattern)
5. **MSMQ Dependency**: Requires adapter package with potential limitations

**Estimated Effort by Phase**:

| Phase | Relative Complexity | Critical Path |
|-------|---------------------|---------------|
| Phase 1: Project Structure | Medium | Yes |
| Phase 2: Packages | Low | Yes |
| Phase 3: App Structure | Medium | Yes |
| Phase 4: Code Migration | High | Yes |
| Phase 5: Build/Compile | High | Yes |
| Phase 6: Testing | High | Yes |
| Phase 7: Finalization | Low | No |

**Dependencies**: Each phase depends on previous phase completion. No parallelization possible.

---

## Source Control Strategy

### Recommended Approach: Single Atomic Commit

**Rationale**:
- All-at-once strategy means changes are interdependent
- Intermediate states are non-functional
- Single commit keeps history clean
- Easy rollback if needed

**Branch Structure**:
```
master (current .NET Framework 4.8)
└── backup/pre-net9-upgrade (safety backup)
    └── upgrade/net9-migration (working branch)
```

**Commit Message Template**:
```
Upgrade ContosoUniversity to .NET 9.0

BREAKING CHANGE: Migrate from .NET Framework 4.8 to .NET 9.0

- Convert project to SDK-style format
- Migrate ASP.NET MVC 5 to ASP.NET Core MVC
- Update Entity Framework Core 3.1 → 9.0
- Replace Web.config with appsettings.json
- Replace Global.asax with Program.cs
- Update all controllers and views to ASP.NET Core APIs
- Reorganize static files under wwwroot/
- Update MSMQ usage with .NET Core compatible adapter
- Update all NuGet packages to latest compatible versions

Closes #[issue-number]

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

### Post-Merge Strategy

After successful upgrade and testing:
```bash
# Merge to master
git checkout master
git merge --no-ff upgrade/net9-migration
git push origin master

# Tag the release
git tag -a v2.0.0-net9 -m "Upgraded to .NET 9.0"
git push origin v2.0.0-net9

# Delete working branch (optional)
git branch -d upgrade/net9-migration

# Keep backup branch for reference
```

---

## Success Criteria

### Technical Criteria

✅ **Project Configuration**:
- ContosoUniversity.csproj targets net9.0
- SDK-style project format
- All packages .NET 9-compatible

✅ **Build**:
- `dotnet build` succeeds with zero errors
- Zero warnings (or only acceptable warnings documented)

✅ **Runtime**:
- Application starts: `dotnet run` succeeds
- All pages load without errors
- No unhandled exceptions in logs

✅ **Functionality**:
- All CRUD operations work (Students, Courses, Instructors, Departments)
- Pagination works correctly
- Search/filter functions operate
- Entity Framework queries execute successfully
- Database initialization succeeds

✅ **MSMQ Integration**:
- Notification queue accessible
- Messages can be sent
- Messages can be received (if consumer exists)
- No runtime errors related to messaging

✅ **Configuration**:
- Connection string works
- All appsettings.json keys read correctly
- Environment-specific configuration works

✅ **Static Files**:
- CSS loads and applies correctly
- JavaScript functions properly
- Uploaded files accessible

### Acceptance Criteria

The upgrade is **complete and successful** when:
1. All technical criteria met
2. Manual testing of core workflows passes
3. No critical bugs identified
4. Performance comparable to .NET Framework version
5. Code committed to version control
6. Documentation updated

### Post-Upgrade Validation Checklist

- [ ] Project builds without errors
- [ ] Application starts and serves requests
- [ ] Home page displays correctly
- [ ] Students CRUD operations work
- [ ] Courses CRUD operations work
- [ ] Instructors CRUD operations work
- [ ] Departments CRUD operations work
- [ ] Database queries return correct data
- [ ] Pagination functions correctly
- [ ] File uploads work
- [ ] MSMQ notifications send successfully
- [ ] Configuration values read from appsettings.json
- [ ] Logging works correctly
- [ ] Error handling functions properly
- [ ] Static files (CSS, JS) load correctly
- [ ] No console errors in browser
- [ ] No unhandled exceptions in application logs

---

## Known Limitations and Considerations

### MSMQ on .NET Core

**Limitation**: MSMQ.Messaging is a community adapter, not official Microsoft support.

**Considerations**:
- Test thoroughly before production deployment
- MSMQ only works on Windows
- Consider migrating to Azure Service Bus or RabbitMQ for long-term maintainability
- Document MSMQ as technical debt for future refactoring

### Web.config Features Not Directly Transferable

Some Web.config features require different approaches in ASP.NET Core:
- `<system.web>` sections → Configure in Program.cs or middleware
- `<httpModules>` → Use ASP.NET Core middleware
- `<customErrors>` → Use exception handling middleware
- `<authentication>` → Use ASP.NET Core authentication services

This plan addresses standard configuration; custom modules would need individual assessment.

### Browser Compatibility

- ASP.NET Core uses different bundling/minification
- Verify JavaScript compatibility with current browser requirements
- Test thoroughly in target browsers

### Deployment Changes

- IIS hosting requires ASP.NET Core Hosting Bundle
- IIS configuration via web.config now minimal (hosting settings only)
- Application settings no longer in web.config
- Consider containerization (Docker) for modern deployment

---

## Next Steps After Upgrade

### Immediate Post-Upgrade Tasks

1. **Performance Baseline**: Measure and document application performance
2. **Security Review**: Verify security settings in ASP.NET Core context
3. **Monitoring Setup**: Implement logging and monitoring for .NET 9 app
4. **Team Training**: Brief team on ASP.NET Core differences

### Future Enhancements

Consider these improvements after successful upgrade:

1. **MSMQ Migration**: Replace with Azure Service Bus or RabbitMQ
2. **Authentication**: Add ASP.NET Core Identity if user auth needed
3. **API Layer**: Add RESTful API endpoints for mobile/SPA clients
4. **Containerization**: Create Dockerfile for container deployment
5. **CI/CD**: Set up automated build and deployment pipelines
6. **Testing**: Add comprehensive unit and integration tests
7. **Null Safety**: Enable and address nullable reference type warnings
8. **Performance**: Optimize with async/await patterns throughout

---

## Appendix A: Package Versions Reference

### Required Packages (Minimum)

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
<PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.0" />
<PackageReference Include="MSMQ.Messaging" Version="1.3.0" />
```

### Optional Packages

```xml
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="Microsoft.Identity.Client" Version="4.66.2" />
```

---

## Appendix B: Breaking Changes Reference

### Entity Framework Core 3.1 → 9.0

- Client vs server evaluation behavior changes
- LINQ query translation improvements
- Lazy loading requires explicit configuration
- Value converters may need updates

**Mitigation**: Test all queries; review EF Core 9.0 breaking changes documentation.

### ASP.NET MVC 5 → ASP.NET Core MVC

- Different dependency injection model
- Different routing configuration
- Different bundling/minification approach
- Different configuration system
- Different hosting model

**Mitigation**: Systematic code review and testing.

---

## Appendix C: Useful Commands

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run application
dotnet run

# Run with specific environment
dotnet run --environment Production

# EF Core migrations
dotnet ef migrations add MigrationName
dotnet ef database update
dotnet ef migrations list

# Clean build
dotnet clean
dotnet build

# Publish for deployment
dotnet publish -c Release -o ./publish

# Check installed SDKs
dotnet --list-sdks

# Check project info
dotnet --info
```

---

## Plan Execution Instructions

This plan is designed for autonomous execution by an agent. The executing agent should:

1. **Follow phases sequentially** - Do not skip phases or reorder steps
2. **Complete each phase fully** before moving to next
3. **Validate after each phase** using build commands or tests
4. **Document any deviations** from the plan with justification
5. **Report blocking issues** if unable to proceed
6. **Use atomic commits** or single commit as specified
7. **Do not pause for user confirmation** - execute fully automatically
8. **Handle errors gracefully** - attempt resolution, document if cannot proceed

### Execution Flow

```
START
  ↓
Phase 1: Project Structure Modernization
  ↓
Phase 2: Package and Dependency Migration
  ↓
Phase 3: Application Structure Transformation
  ↓
Phase 4: Code Migration and Breaking Changes
  ↓
Phase 5: Build and Compilation
  ↓ (if build fails, iterate fixes)
Phase 6: Testing and Validation
  ↓ (if tests fail, iterate fixes)
Phase 7: Finalization
  ↓
COMPLETE
```

Each phase must complete successfully before proceeding. If a phase cannot complete, the agent should document the blocking issue and pause for human intervention.

---

*This upgrade plan provides comprehensive guidance for migrating ContosoUniversity from .NET Framework 4.8 to .NET 9.0. Follow systematically for successful upgrade.*
