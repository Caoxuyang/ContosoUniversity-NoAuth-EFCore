# .NET 10 Upgrade Plan for dotnet-demo-s3

## Executive Summary

This plan outlines the upgrade of the dotnet-demo-s3 solution from .NET 6.0 to .NET 10.0 (Long Term Support). The solution contains a single console application project with minimal complexity and no inter-project dependencies.

### Metrics

| Metric | Value |
|--------|-------|
| **Projects to Upgrade** | 1 |
| **Current Framework** | .NET 6.0 |
| **Target Framework** | .NET 10.0 (LTS) |
| **Total Lines of Code** | 225 |
| **NuGet Packages** | 4 (2 require upgrade) |
| **API Breaking Changes** | 0 |
| **Estimated Impact** | Low |

### Classification

**Upgrade Complexity**: 🟢 **Low**

**Rationale**: Single SDK-style project, minimal code footprint, no breaking API changes, AWS SDK packages remain compatible, Microsoft.Extensions packages have straightforward upgrade paths.

---

## Upgrade Strategy

### Selected Approach: All-At-Once

**Justification**: This solution consists of a single project with no dependencies, making the All-At-Once strategy the natural and optimal choice. The atomic nature of this upgrade minimizes risk and testing overhead.

**Key Characteristics**:
- Single operation across all solution components
- Atomic commit preferred for version control
- Minimal coordination overhead (single project)
- Fastest time to completion

---

## Dependency Analysis

### Project Structure

```
dotnet-demo-s3.sln
└── dotnet-demo-s3.csproj (Console Application)
    ├── Program.cs
    └── S3Service.cs
```

### Dependency Graph

The solution contains only one project with no inter-project dependencies. There are no dependency constraints to consider for upgrade ordering.

**Upgrade Order**: Single-phase execution (only one project exists)

---

## Project-by-Project Upgrade Plans

### dotnet-demo-s3.csproj

#### Project Overview

| Property | Value |
|----------|-------|
| **Type** | Console Application |
| **Current TFM** | net6.0 |
| **Target TFM** | net10.0 |
| **SDK Style** | Yes |
| **Files** | 2 code files (Program.cs, S3Service.cs) |
| **LOC** | 225 |
| **Dependencies** | None (no project references) |

#### Package Updates Required

| Package | Current Version | Target Version | Status |
|---------|-----------------|----------------|---------|
| Microsoft.Extensions.Configuration | 6.0.1 | 10.0.3 | ⚠️ Upgrade Required |
| Microsoft.Extensions.Configuration.Json | 6.0.0 | 10.0.3 | ⚠️ Upgrade Required |
| AWSSDK.S3 | 3.7.300.2 | 3.7.300.2 | ✅ Compatible |
| AWSSDK.Extensions.NETCore.Setup | 3.7.7 | 3.7.7 | ✅ Compatible |

#### Upgrade Steps

1. **Update Target Framework**
   - File: `dotnet-demo-s3.csproj`
   - Change: `<TargetFramework>net6.0</TargetFramework>` → `<TargetFramework>net10.0</TargetFramework>`

2. **Update Package References**
   - File: `dotnet-demo-s3.csproj`
   - Update `Microsoft.Extensions.Configuration` from `6.0.1` to `10.0.3`
   - Update `Microsoft.Extensions.Configuration.Json` from `6.0.0` to `10.0.3`
   - Keep AWS SDK packages at current versions (already compatible)

3. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

4. **Build Project**
   ```bash
   dotnet build
   ```

5. **Test Application**
   - Run the console application
   - Verify S3 operations work correctly
   - Test all menu options (upload, download, update, delete, list)

#### Expected Breaking Changes

**None identified**. The assessment found zero API compatibility issues. The code uses:
- AWS SDK (AWSSDK.S3, AWSSDK.Extensions.NETCore.Setup): Fully compatible with .NET 10
- Microsoft.Extensions.Configuration: Standard dependency injection patterns that remain stable across .NET versions
- Standard BCL APIs (Console, StreamReader, Task): No breaking changes

#### Validation Checklist

- [ ] Project builds without errors
- [ ] Project builds without warnings
- [ ] Application runs successfully
- [ ] S3 client initialization works
- [ ] Upload operation functions correctly
- [ ] Download operation functions correctly
- [ ] Update operation functions correctly
- [ ] Delete operation functions correctly
- [ ] List operation functions correctly
- [ ] No runtime exceptions occur

---

## Package Update Reference

### Complete Package Inventory

| Package | Current | Target | Action | Projects |
|---------|---------|--------|--------|----------|
| Microsoft.Extensions.Configuration | 6.0.1 | 10.0.3 | Upgrade | dotnet-demo-s3.csproj |
| Microsoft.Extensions.Configuration.Json | 6.0.0 | 10.0.3 | Upgrade | dotnet-demo-s3.csproj |
| AWSSDK.S3 | 3.7.300.2 | - | No change | dotnet-demo-s3.csproj |
| AWSSDK.Extensions.NETCore.Setup | 3.7.7 | - | No change | dotnet-demo-s3.csproj |

### Package Update Details

#### Microsoft.Extensions.Configuration (6.0.1 → 10.0.3)

**Purpose**: Configuration abstraction for .NET applications

**Breaking Changes**: None expected. This is a stable API surface that maintains backward compatibility across major versions.

**Migration Notes**: Direct version update. No code changes required.

**Projects Affected**: dotnet-demo-s3.csproj

#### Microsoft.Extensions.Configuration.Json (6.0.0 → 10.0.3)

**Purpose**: JSON configuration provider

**Breaking Changes**: None expected. Configuration file parsing and binding remain consistent.

**Migration Notes**: Direct version update. Existing appsettings.json continues to work without modification.

**Projects Affected**: dotnet-demo-s3.csproj

#### AWSSDK.S3 (3.7.300.2 - No Update)

**Purpose**: AWS S3 client library

**Compatibility**: Fully compatible with .NET 10.0. AWS SDK for .NET supports .NET 6+ including .NET 10.

**Projects Affected**: dotnet-demo-s3.csproj

#### AWSSDK.Extensions.NETCore.Setup (3.7.7 - No Update)

**Purpose**: AWS SDK integration with .NET Core dependency injection

**Compatibility**: Fully compatible with .NET 10.0.

**Projects Affected**: dotnet-demo-s3.csproj

---

## Breaking Changes Catalog

### Assessment Summary

**Total Breaking Changes**: 0

The assessment analyzed all APIs used in the codebase and found no binary incompatibilities, source incompatibilities, or behavioral changes when upgrading from .NET 6.0 to .NET 10.0.

### Code Patterns Used

The application uses the following stable patterns:
- AWS S3 SDK client initialization and operations
- Async/await patterns (no breaking changes)
- Console I/O operations (stable API surface)
- StreamReader for response handling (unchanged)
- Standard exception handling patterns

No code modifications are required for framework compatibility.

---

## Testing Strategy

### Multi-Level Testing Approach

Since this is a single-project solution with no automated test suite, testing will be manual and focused on functional validation.

#### Project-Level Testing

**dotnet-demo-s3.csproj**:
1. Build validation: Ensure clean build with no errors or warnings
2. Startup validation: Application launches successfully
3. S3 client initialization: Verify AWS SDK creates client correctly
4. Functional testing: Execute each menu operation
   - Option 1: Upload text object
   - Option 2: Download object
   - Option 3: Update object
   - Option 4: Delete object
   - Option 5: List objects
   - Option 6: Exit gracefully

#### Full Solution Testing

Since there's only one project, project-level and solution-level testing are equivalent.

**Acceptance Criteria**:
- [ ] Solution builds successfully with `dotnet build`
- [ ] Application runs without runtime errors
- [ ] All S3 CRUD operations complete successfully
- [ ] Console output displays expected messages
- [ ] Application exits cleanly

#### Testing Checklist

**Build Validation**:
- [ ] `dotnet restore` completes without errors
- [ ] `dotnet build` succeeds with no errors
- [ ] No compiler warnings related to framework compatibility
- [ ] No NuGet package restoration issues

**Runtime Validation**:
- [ ] Application starts and displays menu
- [ ] AWS S3 client initializes correctly
- [ ] Upload operation creates objects in S3
- [ ] Download operation retrieves object content
- [ ] Update operation modifies existing objects
- [ ] Delete operation removes objects
- [ ] List operation displays bucket contents
- [ ] Application handles invalid input gracefully
- [ ] Application exits without errors

---

## Risk Management

### Risk Assessment

#### Overall Risk Level: 🟢 **Low**

**Justification**:
- Small codebase (225 LOC)
- No breaking API changes identified
- AWS SDK packages are stable and compatible
- Microsoft.Extensions packages have proven upgrade paths
- Single project eliminates coordination risk
- No automated tests to break
- Console application (not production-critical)

### Identified Risks

#### Risk 1: AWS SDK Compatibility Issues

**Likelihood**: Very Low  
**Impact**: Medium  
**Description**: While AWS SDK reports as compatible, untested runtime interactions with .NET 10 could surface issues.

**Mitigation**:
- Test all S3 operations thoroughly after upgrade
- Verify AWS credential resolution works correctly
- Check error handling paths
- Validate async operation completion

**Rollback Plan**: Revert target framework to net6.0 in project file, restore packages

#### Risk 2: Build Configuration Issues

**Likelihood**: Very Low  
**Impact**: Low  
**Description**: Build properties or compiler options may behave differently in .NET 10.

**Mitigation**:
- Review compiler warnings after initial build
- Verify output artifacts are generated correctly
- Test application startup and shutdown

**Rollback Plan**: Revert project file changes via source control

#### Risk 3: Implicit Using Directives Changes

**Likelihood**: Very Low  
**Impact**: Low  
**Description**: .NET 10 may add or remove implicit usings, potentially causing ambiguous references.

**Mitigation**:
- Build immediately after framework update
- Address any namespace ambiguity errors
- Project already has `<ImplicitUsings>enable</ImplicitUsings>` set

**Rollback Plan**: Add explicit using directives if conflicts arise

### Rollback Strategy

**Primary Rollback**: Source control revert

Since the upgrade is performed in a dedicated branch (`upgrade-to-NET10`), rollback is straightforward:

```bash
# Option 1: Revert specific commit
git revert <upgrade-commit-hash>

# Option 2: Reset branch to pre-upgrade state
git reset --hard <commit-before-upgrade>

# Option 3: Abandon upgrade branch entirely
git checkout master
git branch -D upgrade-to-NET10
```

**Rollback Steps**:
1. Identify issue severity
2. If critical: execute rollback immediately
3. If non-critical: document issue, attempt fix
4. Restore packages after rollback: `dotnet restore`
5. Verify application functionality on .NET 6.0

**Recovery Time**: < 5 minutes (simple git operations)

---

## Source Control

### Branch Strategy

**Upgrade Branch**: `upgrade-to-NET10`  
**Source Branch**: `master`

### Commit Approach

**Recommended**: Single atomic commit

**Rationale**: 
- Single project with simultaneous changes
- No incremental milestones to preserve
- Simplifies rollback
- Clear upgrade boundary

### Commit Structure

```
feat: Upgrade to .NET 10.0

- Update target framework from net6.0 to net10.0
- Upgrade Microsoft.Extensions.Configuration to 10.0.3
- Upgrade Microsoft.Extensions.Configuration.Json to 10.0.3
- Maintain AWS SDK package versions (compatible)

Tested:
- Build successful
- All S3 operations validated
- No runtime errors

BREAKING CHANGE: Requires .NET 10 SDK to build and run
```

### Pre-Commit Validation

Before committing:
- [ ] Project builds successfully
- [ ] Application runs without errors
- [ ] All functional tests pass
- [ ] No unintended file changes (e.g., bin/, obj/ excluded)

### Post-Upgrade Actions

1. **Create Pull Request** from `upgrade-to-NET10` to `master`
2. **Document Changes** in PR description (reference this plan)
3. **Request Review** if team workflow requires
4. **Merge** after approval and final validation
5. **Tag Release** (optional): `git tag v2.0.0-net10`

---

## Complexity Assessment

### Overall Complexity: 🟢 **Low**

### Complexity Factors

| Factor | Rating | Notes |
|--------|--------|-------|
| **Project Count** | Low | Single project |
| **Codebase Size** | Low | 225 LOC |
| **Dependency Depth** | Low | No project dependencies |
| **Package Updates** | Low | 2 packages, straightforward updates |
| **Breaking Changes** | Low | Zero identified |
| **Test Coverage** | Low | No automated tests (manual testing required) |
| **API Surface** | Low | Console app with file I/O and AWS SDK calls |
| **Configuration Complexity** | Low | Simple project file, no multi-targeting |

### Effort Estimate

**Relative Complexity**: Low

**Phases**:
1. **Preparation**: Low (verify .NET 10 SDK installed)
2. **Execution**: Low (update 2 lines in csproj, update 2 package versions)
3. **Testing**: Low (manual functional testing of 6 menu operations)
4. **Validation**: Low (single project build and runtime check)

---

## Prerequisites

Before beginning the upgrade, ensure the following prerequisites are met:

### .NET 10 SDK Installation

**Requirement**: .NET 10 SDK must be installed on the development machine.

**Verification**:
```bash
dotnet --list-sdks
```

**Expected Output**: Should include `10.0.x` in the list.

**Installation** (if needed):
- **Windows**: Download from https://dotnet.microsoft.com/download/dotnet/10.0 or use `winget install Microsoft.DotNet.SDK.10`
- **Linux**: Follow https://learn.microsoft.com/dotnet/core/install/linux
- **macOS**: Download from https://dotnet.microsoft.com/download/dotnet/10.0 or use `brew install --cask dotnet-sdk`

### Development Environment

**Recommended IDE**: Visual Studio 2022 17.12+ or Visual Studio Code with C# extension

**Required Tools**:
- Git (for source control operations)
- AWS credentials configured (for testing S3 operations)

### AWS Configuration

**S3 Bucket Access**: Ensure valid AWS credentials are available and have permissions to:
- PutObject
- GetObject
- DeleteObject
- ListObjects

**Credentials Setup**: Configure via AWS CLI, environment variables, or IAM role (if running on EC2/ECS)

### Source Control

**Current Branch**: Ensure you're on the `upgrade-to-NET10` branch before starting.

```bash
git checkout upgrade-to-NET10
```

**Clean Working Directory**: Commit or stash any pending changes before beginning upgrade.

---

## Success Criteria

The upgrade is considered complete and successful when ALL of the following criteria are met:

### Technical Criteria

- [x] **Target Framework Updated**: Project file specifies `<TargetFramework>net10.0</TargetFramework>`
- [ ] **Packages Updated**: All required package versions match the Package Update Reference table
- [ ] **Clean Build**: `dotnet build` completes with zero errors
- [ ] **No Warnings**: Build produces no framework-related warnings
- [ ] **Dependency Resolution**: `dotnet restore` succeeds without conflicts
- [ ] **Application Starts**: Console application launches successfully
- [ ] **AWS SDK Initializes**: S3 client creates without exceptions

### Functional Criteria

- [ ] **Upload Operation**: Successfully uploads text content to S3
- [ ] **Download Operation**: Successfully retrieves object content from S3
- [ ] **Update Operation**: Successfully modifies existing S3 objects
- [ ] **Delete Operation**: Successfully removes objects from S3
- [ ] **List Operation**: Successfully displays bucket contents
- [ ] **Error Handling**: Application handles invalid input gracefully
- [ ] **Exit Operation**: Application terminates cleanly

### Quality Criteria

- [ ] **No Runtime Errors**: Application runs without exceptions during normal operations
- [ ] **Performance**: No noticeable performance degradation compared to .NET 6.0 version
- [ ] **Memory**: No memory leaks or excessive allocations observed

### Documentation Criteria

- [ ] **Commit Created**: Changes committed with descriptive message
- [ ] **Pull Request**: PR created with upgrade details documented
- [ ] **README Updated**: If necessary, update README with .NET 10 requirement

### Validation Steps

1. Execute `dotnet build` and confirm zero errors
2. Run application and test each menu operation
3. Verify console output matches expected behavior
4. Check for any error messages or stack traces
5. Confirm clean application exit

**Final Sign-Off**: All criteria above must be checked before considering the upgrade complete.

---

## Appendix

### Reference Materials

- [.NET 10 Release Notes](https://github.com/dotnet/core/tree/main/release-notes/10.0)
- [Breaking Changes in .NET 10](https://learn.microsoft.com/dotnet/core/compatibility/10.0)
- [AWS SDK for .NET - .NET 10 Support](https://github.com/aws/aws-sdk-net)
- [Microsoft.Extensions.Configuration Documentation](https://learn.microsoft.com/dotnet/core/extensions/configuration)

### File Locations

- **Project File**: `dotnet-demo-s3.csproj`
- **Source Files**: `Program.cs`, `S3Service.cs`
- **Configuration**: `appsettings.json`
- **Solution File**: `dotnet-demo-s3.sln`

### Key Contacts

- **Assessment Document**: `.github/upgrades/scenarios/new-dotnet-version_f186f3/assessment.md`
- **Plan Document**: `.github/upgrades/scenarios/new-dotnet-version_f186f3/plan.md`

---

*This plan was generated based on comprehensive assessment of the dotnet-demo-s3 solution. All version numbers, file paths, and package details are derived from actual project analysis.*
