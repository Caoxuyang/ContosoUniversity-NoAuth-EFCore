# Task 013 - Migrate Configuration from Web.config to appsettings.json

## Summary

Migrated application configuration from Web.config to ASP.NET Core appsettings.json format.

## Changes Made

### Files Created
- `appsettings.json` - Main configuration file with:
  - `Logging` section with log levels
  - `AllowedHosts: "*"`
  - `ConnectionStrings.DefaultConnection` - SQL Server connection string (migrated from Web.config)
  - `NotificationQueuePath` - App setting migrated from Web.config
- `appsettings.Development.json` - Development-specific overrides

### Files Modified
- `Data/SchoolContextFactory.cs` - Updated to implement `IDesignTimeDbContextFactory<SchoolContext>` using `IConfiguration` to read from appsettings.json instead of `ConfigurationManager`
- `Services/NotificationService.cs` - Removed `System.Configuration.ConfigurationManager` dependency; replaced MSMQ with in-memory `ConcurrentQueue<Notification>` for .NET Core compatibility

### Configuration Migration
| Old (Web.config) | New (appsettings.json) |
|---|---|
| `<connectionStrings>` | `ConnectionStrings` section |
| `<appSettings key="NotificationQueuePath">` | Root-level `NotificationQueuePath` key |
| `<compilation debug="true">` | Set via environment |
| `<httpRuntime maxRequestLength>` | Configure in `Program.cs` if needed |

### Web.config
Web.config retained with minimal IIS-required content.
