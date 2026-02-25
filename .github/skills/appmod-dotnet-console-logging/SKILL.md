---
name: dotnet-console-logging
description: Migrate file-based logging to console logging for cloud applications. Use when migrating from log4net, NLog, Serilog with file appenders, or any file-based logging to stdout/stderr console logging for Azure App Service, Azure Container Apps, or Kubernetes deployments.
---

# Console Logging

Applications deployed to cloud environments like Azure App Service, Azure Container Apps, or Kubernetes should use console logging (stdout/stderr) as the primary logging mechanism. This enables cloud platforms to capture, aggregate, and forward logs to their native logging services.

## Why Console Logging?

- **Cloud Platform Integration**: Azure App Service, Container Apps, and Kubernetes automatically capture stdout/stderr
- **Log Aggregation**: Console logs are easily forwarded to Azure Monitor, Application Insights, or third-party services
- **Container Best Practices**: The [twelve-factor app methodology](https://12factor.net/logs) recommends treating logs as event streams
- **Simplicity**: No file management, rotation, or cleanup required

## ASP.NET Core Configuration

### Basic Console Logging

```csharp
var builder = WebApplication.CreateBuilder(args);

// Console logging is configured by default in ASP.NET Core
// Ensure it's enabled in appsettings.json
builder.Logging.AddConsole();

var app = builder.Build();
```

### appsettings.json Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "Console": {
      "FormatterName": "json",
      "FormatterOptions": {
        "SingleLine": true,
        "IncludeScopes": true,
        "TimestampFormat": "yyyy-MM-dd HH:mm:ss ",
        "UseUtcTimestamp": true,
        "JsonWriterOptions": {
          "Indented": false
        }
      }
    }
  }
}
```

### Structured JSON Logging

For better log parsing in cloud environments, use JSON formatting:

```csharp
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
    options.UseUtcTimestamp = true;
    options.JsonWriterOptions = new JsonWriterOptions
    {
        Indented = false
    };
});
```

## .NET Framework Applications

### Using Microsoft.Extensions.Logging

Add NuGet packages:
```xml
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
```

Configure logging:
```csharp
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Information)
        .AddConsole();
});

ILogger logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("Application started");
```

### Migrating from log4net

Replace file appenders with console appenders:

```xml
<!-- Before: File Appender -->
<appender name="FileAppender" type="log4net.Appender.RollingFileAppender">
    <file value="logs/app.log" />
    <!-- ... -->
</appender>

<!-- After: Console Appender -->
<appender name="ConsoleAppender" type="log4net.Appender.ConsoleAppender">
    <layout type="log4net.Layout.PatternLayout">
        <conversionPattern value="%date [%thread] %-5level %logger - %message%newline" />
    </layout>
</appender>
```

### Migrating from NLog

```xml
<!-- Before: File Target -->
<target name="file" xsi:type="File" fileName="logs/app.log" />

<!-- After: Console Target -->
<target name="console" xsi:type="Console"
        layout="${longdate}|${level:uppercase=true}|${logger}|${message}${exception:format=tostring}" />
```

## Best Practices

1. **Use Structured Logging**: Include contextual information as structured data, not string concatenation
   ```csharp
   // Good
   logger.LogInformation("Processing order {OrderId} for customer {CustomerId}", orderId, customerId);

   // Avoid
   logger.LogInformation($"Processing order {orderId} for customer {customerId}");
   ```

2. **Set Appropriate Log Levels**:
   - `Error`: Application errors that need attention
   - `Warning`: Unexpected situations that don't prevent operation
   - `Information`: High-level application flow
   - `Debug`: Detailed diagnostic information (disable in production)

3. **Include Correlation IDs**: For distributed tracing across services
   ```csharp
   using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
   {
       logger.LogInformation("Processing request");
   }
   ```

4. **Avoid Sensitive Data**: Never log passwords, tokens, or PII
