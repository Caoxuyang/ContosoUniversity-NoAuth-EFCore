---
name: validation-dotnet-check-cves-and-fix
description: Check .NET projects for known security vulnerabilities (CVEs) in dependencies and fix them by updating to secure versions. Requires .NET project with NuGet dependencies.
---

# .NET CVE Check and Fix

## Overview

This skill scans .NET projects for known security vulnerabilities in NuGet dependencies and fixes them by updating to secure versions.

## User Input

- **project-directory** (Mandatory): The root directory of the .NET project to scan
- **migration-folder** (Mandatory): Folder for storing CVE reports and results
- **auto-fix** (Optional): Whether to automatically update vulnerable dependencies (default: true)

## Workflow

TODO: Implement .NET CVE check and fix workflow

## Completion Criteria

1. **Scan Complete**: All NuGet dependencies have been scanned for vulnerabilities
2. **Fixes Applied**: Vulnerable dependencies have been updated to secure versions
3. **Report Generated**: Comprehensive CVE report documents the findings and fixes
