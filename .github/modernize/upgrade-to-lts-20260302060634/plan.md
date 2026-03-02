# .NET 10 Upgrade Plan for ContosoUniversity

## Executive Summary

This plan details the upgrade of ContosoUniversity from .NET Framework 4.8 to .NET 10.0 LTS. The solution contains a single ASP.NET MVC web application with Entity Framework Core 3.1.32 data access, requiring conversion from non-SDK-style to SDK-style project format, significant package updates, and migration of incompatible APIs.

### Key Metrics

- **Projects**: 1 (ContosoUniversity.csproj)
- **Current Framework**: .NET Framework 4.8
- **Target Framework**: .NET 10.0 (LTS)
- **Total LOC**: 3,392
- **Estimated LOC to Modify**: 92+ (2.7% of codebase)
- **Package Updates**: 26 packages requiring upgrade/removal
- **API Issues**: 92 incompatible APIs (63 binary, 29 source incompatible)
- **Upgrade Classification**: **All-At-Once** (single project, moderate complexity)

### Critical Migration Challenges

1. **Project Conversion** (HIGH): Non-SDK-style to SDK-style project format
2. **MSMQ Migration** (HIGH): 59 API issues - requires alternative message queuing
3. **ASP.NET Framework to ASP.NET Core** (HIGH): 16 System.Web API issues
4. **Configuration System** (MEDIUM): 16 issues migrating from Web.config to appsettings.json
5. **Entity Framework Core** (MEDIUM): Upgrade from 3.1.32 to 10.0.3
6. **Security Vulnerability** (HIGH): Microsoft.Data.SqlClient 2.1.4 → 6.1.4

---

## Upgrade Strategy

**Selected Approach**: **All-At-Once**

### Rationale

- Single project eliminates dependency coordination complexity
- All assessment analysis shows compatible upgrade paths
- Homogeneous codebase with consistent patterns
- EF Core 3.1 → 10.0 is a supported upgrade path
- Clear package replacements identified for all incompatible dependencies

###All-At-Once Strategy Execution

This upgrade follows an atomic, coordinated approach:

1. Convert project format and target framework simultaneously
2. Update all package references in a single operation
3. Migrate incompatible APIs and features together
4. Build, fix compilation errors, and test as a complete unit
5. Single commit for entire upgrade (recommended)

---

## Phase 1: Project Structure Modernization

### 1.1 SDK-Style Conversion

**Objective**: Convert ContosoUniversity.csproj from legacy non-SDK-style to modern SDK-style format.

**Tool to Use**: `AppModDotNetUpgrade-convert_project_to_sdk_style`

**Parameters**:
- `solutionPath`: `C:\Users\xuycao\dev\demo\cca-cli-demo\repos\ContosoUniversity-NoAuth-EFCore\ContosoUniversity.sln`
- `projectPath`: `C:\Users\xuycao\dev\demo\cca-cli-demo\repos\ContosoUniversity-NoAuth-EFCore\ContosoUniversity.csproj`

**Expected Outcome**:
- Project file converted to SDK-style format
- `packages.config` migrated to `PackageReference` elements
- Web application properties preserved
- Target framework set to `net10.0`

**Verification**:
- Project file starts with `<Project Sdk="Microsoft.NET.Sdk.Web">`
- `packages.config` file removed
- All package references now in project file

### 1.2 Target Framework Update

**Action**: Ensure `<TargetFramework>` is set to `net10.0` in ContosoUniversity.csproj.

**Expected Content**:
```xml
<TargetFramework>net10.0</TargetFramework>
```

**Verification**: Confirm target framework property is present and correct.

---

## Phase 2: Dependency Updates

### 2.1 Package Updates - Entity Framework Core Ecosystem

Update all Microsoft.EntityFrameworkCore packages from 3.1.32 to 10.0.3:

| Package | Current | Target |
|---------|---------|--------|
| Microsoft.EntityFrameworkCore | 3.1.32 | 10.0.3 |
| Microsoft.EntityFrameworkCore.Abstractions | 3.1.32 | 10.0.3 |
| Microsoft.EntityFrameworkCore.Analyzers | 3.1.32 | 10.0.3 |
| Microsoft.EntityFrameworkCore.Relational | 3.1.32 | 10.0.3 |
| Microsoft.EntityFrameworkCore.SqlServer | 3.1.32 | 10.0.3 |
| Microsoft.EntityFrameworkCore.Tools | 3.1.32 | 10.0.3 |

**Action**: Update all PackageReference versions in ContosoUniversity.csproj simultaneously.

### 2.2 Package Updates - Microsoft.Extensions Ecosystem

Update all Microsoft.Extensions packages from 3.1.32 to 10.0.3:

| Package | Current | Target |
|---------|---------|--------|
| Microsoft.Extensions.Caching.Abstractions | 3.1.32 | 10.0.3 |
| Microsoft.Extensions.Caching.Memory | 3.1.32 | 10.0.3 |
| Microsoft.Extensions.Configuration | 3.1.32 | 10.0.3 |
| Microsoft.Extensions.Configuration.Abstractions | 3.1.32 | 10.0.3 |
| Microsoft.Extensions.Configuration.Binder | 3.1.32 | 10.0.3 |
| Microsoft.Extensions.DependencyInjection | 3.1.32 | 10.0.3 |
| Microsoft.Extensions.DependencyInjection.Abstractions | 3.1.32 | 10.0.3 |
| Microsoft.Extensions.Logging | 3.1.32 | 10.0.3 |
| Microsoft.Extensions.Logging.Abstractions | 3.1.32 | 10.0.3 |
| Microsoft.Extensions.Options | 3.1.32 | 10.0.3 |
| Microsoft.Extensions.Primitives | 3.1.32 | 10.0.3 |

**Action**: Update all PackageReference versions simultaneously.

### 2.3 Package Updates - Security and Core Libraries

| Package | Current | Target | Priority |
|---------|---------|--------|----------|
| **Microsoft.Data.SqlClient** | **2.1.4** | **6.1.4** | **CRITICAL - Security Vulnerability** |
| Microsoft.Bcl.AsyncInterfaces | 1.1.1 | 10.0.3 | High |
| Microsoft.Bcl.HashCode | 1.1.1 | 6.0.0 | Medium |
| System.Collections.Immutable | 1.7.1 | 10.0.3 | Medium |
| System.Diagnostics.DiagnosticSource | 4.7.1 | 10.0.3 | Medium |
| System.Runtime.CompilerServices.Unsafe | 4.5.3 | 6.1.2 | Medium |
| Newtonsoft.Json | 13.0.3 | 13.0.4 | Low |

**Action**: Update all PackageReference versions simultaneously.

### 2.4 Package Removals - Framework-Included Functionality

Remove packages whose functionality is now included in .NET 10 framework:

| Package | Reason |
|---------|--------|
| Microsoft.AspNet.Mvc | Included in ASP.NET Core framework |
| Microsoft.AspNet.Razor | Included in ASP.NET Core framework |
| Microsoft.AspNet.WebPages | Included in ASP.NET Core framework |
| Microsoft.AspNet.Web.Optimization | Not compatible - replace with TagHelpers |
| Microsoft.CodeDom.Providers.DotNetCompilerPlatform | Included in framework |
| Microsoft.Web.Infrastructure | Included in framework |
| NETStandard.Library | Included in framework |
| System.Buffers | Included in framework |
| System.ComponentModel.Annotations | Included in framework |
| System.Memory | Included in framework |
| System.Numerics.Vectors | Included in framework |
| System.Threading.Tasks.Extensions | Included in framework |

**Action**: Remove all `<PackageReference>` elements for these packages from ContosoUniversity.csproj.

### 2.5 Package Replacements

| Old Package | Action | New Package |
|-------------|--------|-------------|
| Antlr (3.4.1.9004) | Replace | Antlr4 (4.6.6) |
| Microsoft.Identity.Client (4.21.1) | Update | Latest stable version (deprecated warning) |

**Action**:
1. Remove `<PackageReference Include="Antlr" Version="3.4.1.9004" />`
2. Add `<PackageReference Include="Antlr4" Version="4.6.6" />`
3. Update Microsoft.Identity.Client to latest stable (check NuGet.org for current version)

### 2.6 Package Retention - Compatible Libraries

Keep these packages at current versions (already compatible):

- bootstrap (5.3.3)
- jQuery (3.7.1)
- jQuery.Validation (1.21.0)
- Microsoft.jQuery.Unobtrusive.Validation (4.0.0)
- Modernizr (2.6.2)
- WebGrease (1.5.2)
- Microsoft.Data.SqlClient.SNI.runtime (2.1.1)

**Action**: No changes required for these packages.

### 2.7 Dependency Restore

**Action**: After all package updates complete:
```powershell
dotnet restore ContosoUniversity.sln
```

**Expected Outcome**: All packages restored without conflicts.

**Troubleshooting**: If restore fails due to authentication for private NuGet feeds, use `AppModDotNetUpgrade-authenticate_nuget_feed` tool.

---

## Phase 3: Application Migration to ASP.NET Core

### 3.1 Global.asax to Program.cs Migration

**Affected File**: `Global.asax.cs`

**Current Pattern**: ASP.NET MVC application initialization in `Global.asax.cs` with Application_Start method.

**Target Pattern**: ASP.NET Core Program.cs with WebApplication builder pattern.

**Actions**:
1. Create new `Program.cs` file with ASP.NET Core startup configuration
2. Migrate route registration from `Global.asax.cs` → `Program.cs` endpoint mapping
3. Migrate dependency injection configuration
4. Migrate Entity Framework DbContext registration
5. Remove `Global.asax` and `Global.asax.cs` files after migration complete

**Breaking Changes**:
- `RouteCollection.MapRoute` → `app.MapControllerRoute`
- `HttpApplication.Application_Start` → `WebApplicationBuilder.Build().Run()`

**Files to Modify**:
- Create: `Program.cs`
- Remove: `Global.asax`, `Global.asax.cs`

### 3.2 Web.config to appsettings.json Migration

**Affected File**: `Web.config`

**Migration Steps**:
1. Extract connection strings from `<connectionStrings>` section
2. Extract app settings from `<appSettings>` section
3. Create `appsettings.json` with equivalent configuration
4. Create `appsettings.Development.json` for development overrides
5. Update code referencing `ConfigurationManager.AppSettings` → `IConfiguration`
6. Update code referencing `ConfigurationManager.ConnectionStrings` → `IConfiguration`

**Files to Create**:
- `appsettings.json`
- `appsettings.Development.json`

**Files to Modify**:
- All files using `System.Configuration.ConfigurationManager` (16 source incompatibilities)

**Configuration Access Pattern Change**:
```csharp
// Old
string value = ConfigurationManager.AppSettings["key"];
string connStr = ConfigurationManager.ConnectionStrings["name"].ConnectionString;

// New
string value = _configuration["key"];
string connStr = _configuration.GetConnectionString("name");
```

### 3.3 Bundling and Minification Migration

**Affected Package**: Microsoft.AspNet.Web.Optimization (incompatible)

**Current Pattern**: `System.Web.Optimization` with `BundleConfig.cs`

**Target Pattern**: ASP.NET Core TagHelpers or build-time bundling

**Actions**:
1. Remove `Microsoft.AspNet.Web.Optimization` package
2. Replace `@Scripts.Render()` and `@Styles.Render()` with direct `<script>` and `<link>` tags in views
3. Alternative: Add `BuildBundlerMinifier` NuGet package for build-time processing

**Files to Modify**:
- All Razor views using `@Scripts.Render()` or `@Styles.Render()`
- Remove `App_Start\BundleConfig.cs` if present

### 3.4 File Upload Migration (HttpPostedFileBase)

**Affected Files**: Files using `System.Web.HttpPostedFileBase` (4 source incompatibilities)

**Pattern Change**:
```csharp
// Old
public ActionResult Upload(HttpPostedFileBase file)
{
    var length = file.ContentLength;
    var name = file.FileName;
    file.SaveAs(path);
}

// New
public IActionResult Upload(IFormFile file)
{
    var length = file.Length;
    var name = file.FileName;
    using (var stream = new FileStream(path, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }
}
```

**Actions**:
1. Replace `HttpPostedFileBase` → `IFormFile` in controller action parameters
2. Update `.ContentLength` → `.Length`
3. Update `.SaveAs(path)` → `.CopyToAsync(stream)`
4. Add `using Microsoft.AspNetCore.Http;`

### 3.5 Route Registration Migration

**Affected Files**: Files using `System.Web.Routing.RouteCollection` (2 binary incompatibilities)

**Pattern Change**:
```csharp
// Old (Global.asax.cs)
RouteTable.Routes.MapRoute(
    name: "Default",
    url: "{controller}/{action}/{id}",
    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
);

// New (Program.cs)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

**Actions**:
1. Migrate all route definitions from Global.asax.cs to Program.cs
2. Update route pattern syntax from `url:` to `pattern:`
3. Update defaults from `defaults: new { ... }` to inline pattern syntax
4. Update `UrlParameter.Optional` → `{id?}` syntax

---

## Phase 4: MSMQ Migration

**CRITICAL**: MSMQ (System.Messaging) is not supported in .NET Core/.NET. This is the largest API compatibility issue (59 binary incompatible APIs, 64.1% of issues).

### 4.1 Assess MSMQ Usage

**Affected Files**: Files using `System.Messaging.MessageQueue` (20 API usages)

**Actions**:
1. Identify all files using MSMQ APIs
2. Document queue names, message formats, and usage patterns
3. Choose replacement strategy

### 4.2 Migration Options

**Option A: MSMQ.Messaging Package** (Recommended for minimal changes)
- Install `MSMQ.Messaging` NuGet package (community package for .NET Core compatibility)
- Replace `using System.Messaging` → `using MSMQ.Messaging`
- Update queue configuration for .NET Core DI patterns
- **Limitation**: Still requires MSMQ installed on Windows

**Option B: Azure Service Bus** (Recommended for cloud modernization)
- Replace MSMQ with Azure Service Bus
- Benefits: Cloud-native, scalable, managed service
- Requires Azure resources and code refactoring
- Use `dotnet-azure-servicebus` skill for guidance

**Option C: RabbitMQ or Other Message Broker**
- Replace MSMQ with cross-platform message broker
- Benefits: Cross-platform, widely supported
- Requires infrastructure setup

**Recommended**: **Option A (MSMQ.Messaging)** for upgrade phase, consider Option B for future modernization.

### 4.3 MSMQ.Messaging Migration Steps

**Actions**:
1. Add package: `<PackageReference Include="MSMQ.Messaging" Version="<latest>" />`
2. Replace namespace imports:
   ```csharp
   // Old
   using System.Messaging;
   
   // New
   using MSMQ.Messaging;
   ```
3. Update queue configuration for dependency injection
4. Test queue creation, send, and receive operations

**Files to Modify**: All files with System.Messaging references (15 files with incidents)

**API Mappings** (most types remain the same):
- `System.Messaging.MessageQueue` → `MSMQ.Messaging.MessageQueue`
- `System.Messaging.Message` → `MSMQ.Messaging.Message`
- `System.Messaging.MessageQueueAccessRights` → `MSMQ.Messaging.MessageQueueAccessRights`
- (Most APIs unchanged, only namespace changes)

---

## Phase 5: Build and Compilation

### 5.1 Initial Build

**Action**:
```powershell
dotnet build ContosoUniversity.sln --no-restore
```

**Expected Outcome**: Build may fail with compilation errors due to API changes.

### 5.2 Compilation Error Resolution

**Common Expected Errors**:

1. **System.Web API Errors** (16 source incompatibilities)
   - `HttpApplication` → ASP.NET Core equivalents
   - `HttpPostedFileBase` → `IFormFile`
   - `RouteCollection` → Endpoint routing

2. **Configuration API Errors** (16 source incompatibilities)
   - `ConfigurationManager.AppSettings` → `IConfiguration`
   - `ConfigurationManager.ConnectionStrings` → `IConfiguration.GetConnectionString`

3. **MSMQ API Errors** (59 binary incompatibilities)
   - Verify MSMQ.Messaging package installed and namespaces updated

**Resolution Process**:
1. Fix errors in order of frequency (see assessment Top API Migration Challenges)
2. Use `AppModDotNetUpgrade-get_type_info` tool for detailed type migration guidance
3. Use `AppModDotNetUpgrade-get_member_info` tool for specific member migration guidance
4. Use `AppModDotNetUpgrade-get_namespace_info` tool for namespace migration guidance

### 5.3 Iterative Build-Fix Cycle

**Process**:
1. Run build
2. Identify compilation errors
3. Fix errors by file or category
4. Rebuild
5. Repeat until build succeeds

**Success Criteria**: `dotnet build ContosoUniversity.sln` completes with 0 errors, 0 warnings.

---

## Phase 6: Testing and Validation

### 6.1 Discover Test Projects

**Action**: Use `AppModDotNetUpgrade-discover_test_projects` tool to identify test projects.

**Parameters**:
- `solutionPath`: `C:\Users\xuycao\dev\demo\cca-cli-demo\repos\ContosoUniversity-NoAuth-EFCore\ContosoUniversity.sln`
- `projectPaths`: All project paths in solution

### 6.2 Run Tests

**Action**: If test projects exist:
```powershell
dotnet test ContosoUniversity.sln --no-build
```

**Expected Outcome**: All tests pass.

**If Tests Fail**:
1. Analyze test failures
2. Categorize by root cause (API changes, configuration, MSMQ, etc.)
3. Fix issues
4. Rebuild and retest

### 6.3 Manual Testing Checklist

| Area | Test Case | Expected Result |
|------|-----------|-----------------|
| Application Start | Run application | Application starts without errors |
| Database | Connect to database | Connection successful, EF Core 10 queries work |
| Configuration | Access app settings | Configuration values loaded correctly |
| File Upload | Upload a file | File uploads and saves correctly |
| Routing | Navigate to pages | All routes resolve correctly |
| MSMQ | Send/receive messages | Queue operations work (if MSMQ migrated) |
| Static Content | Load CSS/JS | Bundling replacement works correctly |

### 6.4 Performance Baseline

**Optional**: Establish performance baseline after upgrade:
- Application startup time
- Database query performance
- Page load times
- Memory usage

**Action**: Document baseline metrics for comparison with pre-upgrade state.

---

## Dependency Analysis

### Project Dependency Order

**Single Project**: ContosoUniversity.csproj has no dependencies or dependents.

**Upgrade Order**: N/A (single project atomic upgrade)

---

## Package Update Reference

### Complete Package Update Table

| Package | Current | Target | Action | Priority |
|---------|---------|--------|--------|----------|
| **Security Updates** |
| Microsoft.Data.SqlClient | 2.1.4 | 6.1.4 | Update | CRITICAL |
| **Entity Framework Core** |
| Microsoft.EntityFrameworkCore | 3.1.32 | 10.0.3 | Update | High |
| Microsoft.EntityFrameworkCore.Abstractions | 3.1.32 | 10.0.3 | Update | High |
| Microsoft.EntityFrameworkCore.Analyzers | 3.1.32 | 10.0.3 | Update | High |
| Microsoft.EntityFrameworkCore.Relational | 3.1.32 | 10.0.3 | Update | High |
| Microsoft.EntityFrameworkCore.SqlServer | 3.1.32 | 10.0.3 | Update | High |
| Microsoft.EntityFrameworkCore.Tools | 3.1.32 | 10.0.3 | Update | High |
| **Microsoft.Extensions** |
| Microsoft.Extensions.Caching.Abstractions | 3.1.32 | 10.0.3 | Update | Medium |
| Microsoft.Extensions.Caching.Memory | 3.1.32 | 10.0.3 | Update | Medium |
| Microsoft.Extensions.Configuration | 3.1.32 | 10.0.3 | Update | Medium |
| Microsoft.Extensions.Configuration.Abstractions | 3.1.32 | 10.0.3 | Update | Medium |
| Microsoft.Extensions.Configuration.Binder | 3.1.32 | 10.0.3 | Update | Medium |
| Microsoft.Extensions.DependencyInjection | 3.1.32 | 10.0.3 | Update | Medium |
| Microsoft.Extensions.DependencyInjection.Abstractions | 3.1.32 | 10.0.3 | Update | Medium |
| Microsoft.Extensions.Logging | 3.1.32 | 10.0.3 | Update | Medium |
| Microsoft.Extensions.Logging.Abstractions | 3.1.32 | 10.0.3 | Update | Medium |
| Microsoft.Extensions.Options | 3.1.32 | 10.0.3 | Update | Medium |
| Microsoft.Extensions.Primitives | 3.1.32 | 10.0.3 | Update | Medium |
| **Core Libraries** |
| Microsoft.Bcl.AsyncInterfaces | 1.1.1 | 10.0.3 | Update | Medium |
| Microsoft.Bcl.HashCode | 1.1.1 | 6.0.0 | Update | Medium |
| System.Collections.Immutable | 1.7.1 | 10.0.3 | Update | Medium |
| System.Diagnostics.DiagnosticSource | 4.7.1 | 10.0.3 | Update | Medium |
| System.Runtime.CompilerServices.Unsafe | 4.5.3 | 6.1.2 | Update | Medium |
| Newtonsoft.Json | 13.0.3 | 13.0.4 | Update | Low |
| **Replacements** |
| Antlr | 3.4.1.9004 | - | Remove | Medium |
| Antlr4 | - | 4.6.6 | Add | Medium |
| Microsoft.Identity.Client | 4.21.1 | Latest | Update | Low |
| MSMQ.Messaging | - | Latest | Add | High |
| **Removals (Framework-Included)** |
| Microsoft.AspNet.Mvc | 5.2.9 | - | Remove | High |
| Microsoft.AspNet.Razor | 3.2.9 | - | Remove | High |
| Microsoft.AspNet.WebPages | 3.2.9 | - | Remove | High |
| Microsoft.AspNet.Web.Optimization | 1.1.3 | - | Remove | High |
| Microsoft.CodeDom.Providers.DotNetCompilerPlatform | 2.0.1 | - | Remove | Medium |
| Microsoft.Web.Infrastructure | 2.0.1 | - | Remove | Medium |
| NETStandard.Library | 2.0.3 | - | Remove | Low |
| System.Buffers | 4.5.1 | - | Remove | Low |
| System.ComponentModel.Annotations | 4.7.0 | - | Remove | Low |
| System.Memory | 4.5.4 | - | Remove | Low |
| System.Numerics.Vectors | 4.5.0 | - | Remove | Low |
| System.Threading.Tasks.Extensions | 4.5.4 | - | Remove | Low |
| **Retain (Compatible)** |
| bootstrap | 5.3.3 | - | Keep | - |
| jQuery | 3.7.1 | - | Keep | - |
| jQuery.Validation | 1.21.0 | - | Keep | - |
| Microsoft.jQuery.Unobtrusive.Validation | 4.0.0 | - | Keep | - |
| Modernizr | 2.6.2 | - | Keep | - |
| WebGrease | 1.5.2 | - | Keep | - |
| Microsoft.Data.SqlClient.SNI.runtime | 2.1.1 | - | Keep | - |

---

## Breaking Changes Catalog

### Binary Incompatible APIs (63 instances)

**High-Impact Breaking Changes**:

1. **System.Messaging (MSMQ)**: 59 instances across 15 files
   - **Mitigation**: Install MSMQ.Messaging package, update namespaces
   - **Alternative**: Migrate to Azure Service Bus or RabbitMQ

2. **System.Web.Routing**: 2 instances
   - **Mitigation**: Migrate RouteCollection to ASP.NET Core endpoint routing
   - **Files**: `Global.asax.cs`

### Source Incompatible APIs (29 instances)

**High-Impact Source Changes**:

1. **System.Configuration.ConfigurationManager**: 16 instances
   - **Mitigation**: Migrate to IConfiguration with appsettings.json
   - **Pattern Change**: `ConfigurationManager.AppSettings["key"]` → `_configuration["key"]`

2. **System.Web.HttpPostedFileBase**: 4 instances
   - **Mitigation**: Replace with `IFormFile`
   - **Property Changes**: `.ContentLength` → `.Length`, `.SaveAs()` → `.CopyToAsync()`

3. **System.Web.HttpApplication**: 1 instance
   - **Mitigation**: Migrate Application_Start logic to Program.cs

### Package Incompatibilities

1. **Microsoft.AspNet.Web.Optimization**: Bundling/minification not supported
   - **Mitigation**: Use TagHelpers or build-time bundling

2. **Microsoft.AspNet.Mvc/Razor/WebPages**: Included in ASP.NET Core framework
   - **Mitigation**: Remove packages, functionality built-in

---

## Risk Management

### High Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| MSMQ functionality breaks | Application messaging fails | High | Implement MSMQ.Messaging package, test all queue operations thoroughly before deployment |
| Configuration migration incomplete | Application fails to start | Medium | Create comprehensive appsettings.json mapping, validate all config access |
| Breaking changes in EF Core 10 | Data access errors | Medium | Review EF Core 10 breaking changes documentation, test all queries |
| Route migration errors | 404 errors for pages | Medium | Map all existing routes, test navigation comprehensively |
| Build errors difficult to resolve | Extended development time | Medium | Use MCP tools (get_type_info, get_member_info) for migration guidance |

### Medium Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| File upload functionality breaks | Upload feature fails | Low | Update HttpPostedFileBase→IFormFile systematically, test uploads |
| Bundling replacement incomplete | Missing CSS/JS resources | Low | Inventory all bundle references, validate static resource loading |
| Test failures | Uncertain upgrade success | Medium | Fix failing tests before considering upgrade complete |
| Performance regression | Slower application | Low | Establish baseline, compare post-upgrade |

### Rollback Strategy

**Single Commit Approach**: All changes in one commit enables clean rollback.

**Rollback Steps**:
1. `git reset --hard HEAD~1` (if not pushed)
2. `git revert <commit-hash>` (if pushed)
3. Return to source branch: `git checkout master`
4. Restore packages: `dotnet restore`
5. Rebuild: `dotnet build`

**Data Rollback**: If database migrations applied, keep backup and rollback script ready.

---

## Complexity Assessment

### Overall Complexity: **High**

**Complexity Factors**:

| Factor | Rating | Justification |
|--------|--------|---------------|
| Project Conversion | High | Non-SDK to SDK-style conversion |
| Framework Jump | Medium | .NET Framework 4.8 → .NET 10 (large gap) |
| MSMQ Migration | High | 64% of API issues, requires alternative solution |
| ASP.NET Framework → Core | High | Complete web framework redesign |
| Package Updates | Medium | 26 packages, mostly straightforward upgrades |
| API Compatibility | Medium | 92 API issues, but clear migration paths |
| Testing Scope | Medium | 56 files, 3,392 LOC to validate |
| Security Updates | High | Critical security vulnerability in SqlClient |

**Estimated Complexity Distribution**:
- Project Structure (15%)
- Package Updates (20%)
- ASP.NET Core Migration (25%)
- MSMQ Migration (25%)
- Build Fixes (10%)
- Testing (5%)

---

## Source Control

### Git Workflow

**Recommended Approach**: Single commit for entire upgrade

**Branch Strategy**:
- Source branch: `master`
- Upgrade branch: `upgrade-to-NET10` (already created)
- All changes on upgrade branch

**Commit Strategy**:

**Option A: Single Comprehensive Commit** (Recommended for All-At-Once)
```
git add .
git commit -m "Upgrade ContosoUniversity to .NET 10.0

- Convert project to SDK-style format
- Update target framework from net48 to net10.0
- Update EF Core from 3.1.32 to 10.0.3
- Update Microsoft.Extensions packages to 10.0.3
- Fix security vulnerability: Microsoft.Data.SqlClient 2.1.4 → 6.1.4
- Migrate ASP.NET MVC to ASP.NET Core
- Replace MSMQ (System.Messaging) with MSMQ.Messaging
- Migrate Web.config to appsettings.json
- Migrate Global.asax to Program.cs
- Replace bundling/minification with direct script references
- Update file upload from HttpPostedFileBase to IFormFile
- Remove framework-included packages
- All tests passing

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

**Option B: Multi-Commit (If rollback granularity needed)**
1. Commit: Project structure changes
2. Commit: Package updates
3. Commit: API migrations
4. Commit: Build fixes
5. Commit: Test fixes

**Merge Strategy**: Merge `upgrade-to-NET10` → `master` after validation complete

### Pre-Merge Checklist

- [ ] All builds succeed
- [ ] All tests pass
- [ ] Manual testing complete
- [ ] No security vulnerabilities remain
- [ ] Code review completed
- [ ] Documentation updated

---

## Success Criteria

### Technical Validation

- [ ] Project converted to SDK-style format
- [ ] Target framework is net10.0
- [ ] All package updates applied (26 packages)
- [ ] All incompatible packages removed or replaced
- [ ] `dotnet restore` succeeds without errors
- [ ] `dotnet build ContosoUniversity.sln` succeeds with 0 errors, 0 warnings
- [ ] All test projects pass (if tests exist)
- [ ] No security vulnerabilities remain in packages
- [ ] Application starts successfully
- [ ] Database connectivity works with EF Core 10
- [ ] Configuration loads from appsettings.json
- [ ] All routes resolve correctly
- [ ] File upload functionality works
- [ ] MSMQ replacement (if applicable) works
- [ ] Static content (CSS/JS) loads correctly

### Functional Validation

- [ ] All existing features work as expected
- [ ] No regressions in functionality
- [ ] Error handling works correctly
- [ ] Logging works correctly
- [ ] Performance is acceptable (no major regressions)

### Code Quality

- [ ] No compiler warnings
- [ ] Code follows .NET 10 best practices
- [ ] No deprecated API usage (warnings)
- [ ] All TODO/FIXME comments resolved

### Documentation

- [ ] Upgrade plan completed
- [ ] Breaking changes documented
- [ ] Configuration changes documented
- [ ] Deployment guide updated (if exists)

---

## Post-Upgrade Recommendations

### Immediate (Within Sprint)

1. **Monitor Application in Production**
   - Watch for runtime errors not caught in testing
   - Monitor performance metrics
   - Check error logs for unexpected issues

2. **Validate MSMQ Replacement**
   - If using MSMQ.Messaging, confirm all messaging scenarios work
   - Consider scheduling Azure Service Bus migration for future sprint

### Short-Term (Next Quarter)

1. **Consider Azure Service Bus Migration**
   - Replace MSMQ.Messaging with cloud-native Azure Service Bus
   - Benefits: Scalability, reliability, cloud-ready
   - Use `dotnet-azure-servicebus` skill for implementation guidance

2. **Review Deprecated Packages**
   - Update Microsoft.Identity.Client to latest stable (currently deprecated)
   - Review all remaining packages for newer versions

3. **Implement Build-Time Bundling**
   - Consider adding BuildBundlerMinifier for production optimization
   - Alternative: Modern front-end build pipeline (webpack, Vite, etc.)

### Long-Term (Next 6-12 Months)

1. **Modernize Front-End**
   - Consider upgrading from jQuery to modern framework (React, Angular, Vue)
   - Implement SPA architecture if appropriate

2. **Cloud Readiness Assessment**
   - Evaluate Azure App Service readiness
   - Consider containerization (Docker)
   - Implement health checks and readiness probes

3. **Security Hardening**
   - Implement Azure Key Vault for secrets management
   - Consider Azure Managed Identity for database authentication
   - Enable Application Insights for monitoring

---

## Tools and Resources

### MCP Tools Used in This Plan

- `AppModDotNetUpgrade-convert_project_to_sdk_style`
- `AppModDotNetUpgrade-analyze_projects`
- `AppModDotNetUpgrade-get_type_info`
- `AppModDotNetUpgrade-get_member_info`
- `AppModDotNetUpgrade-get_namespace_info`
- `AppModDotNetUpgrade-discover_test_projects`
- `AppModDotNetUpgrade-authenticate_nuget_feed` (if needed)

### Available Skills

**Relevant for this upgrade**:
- `sdk-style-conversion`: Project format conversion guidance
- `msmq-migration`: MSMQ to MSMQ.Messaging migration
- `ef-dbcontext-migration`: EF DbContext registration migration
- `sqlclient-migration`: System.Data.SqlClient to Microsoft.Data.SqlClient
- `dotnet-dependency-management`: Package management guidance

**Relevant for future modernization**:
- `dotnet-azure-servicebus`: MSMQ → Azure Service Bus
- `dotnet-azure-keyvault-secret`: Secrets management
- `dotnet-managed-identity`: Azure authentication
- `dotnet-azure-sql-database`: Managed Identity for SQL

### External Documentation

- [EF Core 10 What's New](https://learn.microsoft.com/ef/core/what-is-new/ef-core-10.0/whatsnew)
- [ASP.NET Core Migration Guide](https://learn.microsoft.com/aspnet/core/migration/mvc)
- [.NET 10 Breaking Changes](https://learn.microsoft.com/dotnet/core/compatibility/10.0)
- [MSMQ.Messaging Package](https://www.nuget.org/packages/MSMQ.Messaging)

---

## Execution Sequence Summary

**Phase-by-Phase Execution**:

1. **Phase 1**: Convert project to SDK-style, update target framework
2. **Phase 2**: Update all packages simultaneously (add, update, remove)
3. **Phase 3**: Migrate ASP.NET Framework to ASP.NET Core (Program.cs, appsettings.json, routing, file upload, bundling)
4. **Phase 4**: Migrate MSMQ to MSMQ.Messaging
5. **Phase 5**: Build and fix compilation errors iteratively
6. **Phase 6**: Run tests, perform manual validation

**Atomic Operation**: All changes in one coordinated operation, single commit recommended.

**Timeline**: Relative complexity is **High** - expect multiple iterations of build-fix cycles, thorough testing required.

---

## Notes

- This plan assumes Windows environment with MSMQ installed (for MSMQ.Messaging option)
- If targeting Linux deployment, Azure Service Bus or RabbitMQ must be used instead of MSMQ.Messaging
- Entity Framework Core 10.0.3 includes breaking changes from 3.1.32 - review [EF Core breaking changes](https://learn.microsoft.com/ef/core/what-is-new/ef-core-10.0/breaking-changes) if query behaviors differ
- ASP.NET Core introduces significant architectural differences - expect substantial code changes in startup and middleware configuration
- Security vulnerability in Microsoft.Data.SqlClient 2.1.4 is addressed by upgrading to 6.1.4 (CRITICAL priority)

---

*This plan supports the Execution stage of the .NET 10 upgrade workflow.*
