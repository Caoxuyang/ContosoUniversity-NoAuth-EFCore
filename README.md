# Contoso University - .NET Framework 4.8.2 with Windows Authentication

This project is a refactored version of the Contoso University sample application, converted from ASP.NET Core to ASP.NET MVC 5 targeting .NET Framework 4.8.2 with Windows Authentication enabled.

## Key Changes Made

### 1. Framework Migration
- **From**: ASP.NET Core 2.2 (.NET Core)
- **To**: ASP.NET MVC 5 (.NET Framework 4.8.2)

### 2. Authentication
- **Windows Authentication**: Enabled in `Web.config`
- **Authorization**: All controllers require authentication (configured in `FilterConfig.cs`)
- **Anonymous Access**: Disabled by default

### 3. Entity Framework Migration
- **From**: Entity Framework Core
- **To**: Entity Framework 6.4.4
- **Database Context**: Updated to use EF6 syntax
- **Connection String**: Updated for .NET Framework

### 4. Project Structure
```
ContosoUniversity/
├── App_Start/              # Application startup configuration
├── Controllers/            # MVC Controllers
├── Data/                   # Entity Framework context and initializer
├── Models/                 # Data models and view models
├── Views/                  # Razor views
├── Content/                # CSS and other content
├── Scripts/                # JavaScript files
├── Properties/             # Assembly properties
├── Global.asax             # Application global events
├── Web.config              # Configuration file
└── packages.config         # NuGet packages
```

## Authentication Configuration

### Web.config Authentication Settings
```xml
<system.web>
    <authentication mode="Windows" />
    <authorization>
        <deny users="?" />
    </authorization>
</system.web>
```

### IIS Configuration
- **Anonymous Authentication**: Disabled
- **Windows Authentication**: Enabled
- The application will automatically authenticate users using their Windows credentials

## Database Configuration

The application uses SQL Server LocalDB with the following connection string in `Web.config`:
```xml
<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Data Source=(LocalDb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\ContosoUniversity.mdf;Initial Catalog=ContosoUniversity;Integrated Security=True" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

## Running the Application

1. **Prerequisites**:
   - Visual Studio 2019 or later
   - IIS Express
   - SQL Server LocalDB

2. **Setup**:
   - Open the project in Visual Studio
   - Restore NuGet packages
   - Build the solution
   - Run using IIS Express

3. **Authentication**:
   - The application will prompt for Windows credentials if not automatically authenticated
   - Users must have a valid Windows account to access the application
   - The authenticated user's name will be displayed in the navigation bar

## Features

- **Student Management**: CRUD operations for students with pagination and search
- **Course Management**: Manage courses and their assignments to departments
- **Instructor Management**: Handle instructor assignments and office locations
- **Department Management**: Manage departments and their administrators
- **Statistics**: View enrollment statistics by date

## Security Features

- **Windows Authentication**: Leverages Active Directory/Windows domain authentication
- **Authorization**: All pages require authentication
- **User Identity**: Display authenticated user information
- **Secure by Default**: No anonymous access allowed

## Database Initialization

The application uses Entity Framework 6 Code First with a database initializer that:
- Creates the database if it doesn't exist
- Seeds sample data including students, instructors, courses, and departments
- Handles model changes by recreating the database

## Migration Notes

Key differences from the original ASP.NET Core version:
- Uses synchronous methods instead of async/await patterns
- Entity Framework 6 syntax for database operations
- Traditional ASP.NET MVC 5 dependency injection patterns
- Windows Authentication instead of cookie authentication
- .NET Framework configuration in Web.config instead of appsettings.json
