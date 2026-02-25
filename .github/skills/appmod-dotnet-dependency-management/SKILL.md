---
name: dotnet-dependency-management
description: Guide for .NET dependency management covering both modern SDK-style (.NET Core, .NET 5+) and legacy .NET Framework projects. Use when working with package references, project file structures, packages.config, or migrating between project formats.
---

# .NET Dependency Management Guide

This document serves as a comprehensive guide for understanding and implementing dependency management in .NET projects, covering both modern SDK-style projects and legacy framework projects.

## Modern SDK-Style Projects (.NET Core, .NET 5+)

### Project File Structure
Modern SDK-style .csproj files use a simplified, declarative format:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Package.Name" Version="1.0.0" />
  </ItemGroup>
</Project>
```

### Adding Package References

#### Via Direct Edit
Add a `PackageReference` element to the project file:
```xml
<ItemGroup>
  <PackageReference Include="Package.Name" Version="1.0.0" />
</ItemGroup>
```

### Central Package Management (Modern Approach)
For solutions with multiple projects, use Directory.Packages.props at the solution level:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Package.Name" Version="1.0.0" />
  </ItemGroup>
</Project>
```

Then in project files, omit the Version attribute:
```xml
<ItemGroup>
  <PackageReference Include="Package.Name" />
</ItemGroup>
```

## Legacy .NET Framework Projects

### Project File Structure
Legacy projects use a verbose, MSBuild-based format:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProjectGuid>{GUID-HERE}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>Project.Namespace</RootNamespace>
    <AssemblyName>Project.Name</AssemblyName>
    <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <Deterministic>true</Deterministic>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Core" />
    <Reference Include="Microsoft.CSharp" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="Class1.cs" />
  </ItemGroup>
  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
</Project>
```

### Locate the Legacy .csproj Files
To identify legacy .csproj files in a repository:
   - Powershell command:
     ```powershell
     Get-ChildItem -Path . -Recurse -Include *.csproj |  ForEach-Object { $_.FullName }
     ```
   - Bash command:
     ```bash
     find . -name "*.csproj" -print
     ```

### Adding Package References

#### Via packages.config
Legacy projects use a separate packages.config file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Package.Name" version="1.0.0" targetFramework="net472" />
</packages>
```

Use file editing tools to add or update packages in the .csproj file.

```xml
<ItemGroup>
  <Reference Include="Package.Name, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ab12345cd67890ef">
    <HintPath>..\packages\Package.Name.1.0.0\lib\net472\Package.Name.dll</HintPath>
  </Reference>
</ItemGroup>
```

### Managing References Directly
Legacy projects require explicit management of assembly references:

```xml
<ItemGroup>
  <Reference Include="System.Data" />
  <Reference Include="System.Xml" />
</ItemGroup>
```

### Adding Source Files
In legacy projects, each source file must be explicitly listed:

```xml
<ItemGroup>
  <Compile Include="Models\User.cs" />
  <Compile Include="Services\UserService.cs" />
</ItemGroup>
```

## Important Notes for LLMs

1. **Project File Editing Restrictions**:
   - CLI tools like `dotnet` CANNOT modify legacy .csproj files properly, use `nuget install` instead
   - Use powershell commands to do direct text editing on legacy project files to add .cs files and manage references when necessary

2. **Identifying Project Type**:
   - Modern projects begin with `<Project Sdk="Microsoft.NET.Sdk">`
   - Legacy projects have `<Project ToolsVersion="..." xmlns="http://schemas.microsoft.com/developer/msbuild/2003">`

3. **Framework Detection**:
   - Modern: `<TargetFramework>net7.0</TargetFramework>`
   - Legacy: `<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>`

4. **Migration Considerations**:
   - When migrating from legacy to modern, packages.config must be converted to PackageReference format
   - Some NuGet packages may not be compatible across frameworks
   - Assembly binding redirects may be needed for legacy projects

5. **Dependency Resolution**:
   - Modern projects use PackageReference with transitive dependencies
   - Legacy projects require all dependencies to be explicitly declared
   - Assembly binding redirects are often needed in legacy projects for version conflicts

6. **Package Source Configuration**:
   - Check for NuGet.config at solution or repository root for custom package sources
   - Legacy projects may have package restore disabled and require manual restoration
