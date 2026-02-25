# Contoso University - .NET 9.0 with ASP.NET Core

This project is the Contoso University sample application built with ASP.NET Core targeting .NET 9.0, using Entity Framework Core for data access.

## Framework

- **Runtime**: .NET 9.0
- **Web Framework**: ASP.NET Core (MVC)
- **ORM**: Entity Framework Core 9.0
- **Database**: SQL Server (LocalDB for development)

## Key Changes from .NET Framework 4.8

### 1. Framework Migration
- **From**: ASP.NET MVC 5 (.NET Framework 4.8.2)
- **To**: ASP.NET Core (.NET 9.0)

### 2. Configuration
- **From**: `Web.config` with `<appSettings>` and `<connectionStrings>`
- **To**: `appsettings.json` with the ASP.NET Core configuration system

### 3. Entity Framework Migration
- **From**: Entity Framework 6
- **To**: Entity Framework Core 9.0
- **Database Context**: Updated to use EF Core syntax with async operations

### 4. Application Startup
- **From**: `Global.asax` + `App_Start/` folder
- **To**: `Program.cs` using the minimal hosting model

### 5. Project Structure
```
ContosoUniversity/
 Controllers/            # ASP.NET Core MVC Controllers
 Data/                   # EF Core DbContext and initializer
 Models/                 # Data models and view models
 Views/                  # Razor views
 wwwroot/                # Static files (CSS, JS, images)
 Properties/             # Launch settings
 Program.cs              # Application entry point and startup
 appsettings.json        # Application configuration
 ContosoUniversity.csproj
```

## Prerequisites

- **.NET 9.0 SDK**  [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **SQL Server LocalDB** (included with Visual Studio) or SQL Server instance

## Database Configuration

Connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=ContosoUniversityNoAuthEFCore;Integrated Security=True;TrustServerCertificate=True"
  }
}
```

## Running the Application

1. **Prerequisites**:
   - .NET 9.0 SDK installed
   - SQL Server LocalDB

2. **Setup**:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

3. The application will be available at `http://localhost:5000` (or the port shown in the console).

## Features

- **Student Management**: CRUD operations for students with pagination and search
- **Course Management**: Manage courses and their assignments to departments
- **Instructor Management**: Handle instructor assignments and office locations
- **Department Management**: Manage departments and their administrators
- **Statistics**: View enrollment statistics by date

## Database Initialization

The application uses Entity Framework Core Code First with a database initializer that:
- Creates the database on first run
- Seeds sample data including students, instructors, courses, and departments

## Performance Improvements (.NET 9.0 vs .NET Framework 4.8)

- **Faster startup**: .NET 9.0 has significantly improved cold start times
- **Higher throughput**: Improved HTTP/Kestrel pipeline for better request throughput
- **Lower memory usage**: Reduced memory footprint with .NET 9.0 runtime optimizations
- **Async I/O**: EF Core enables proper async database operations