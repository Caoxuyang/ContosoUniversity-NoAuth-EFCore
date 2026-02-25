---
name: dotnet-azure-redis-cache
description: Migrate caching to Azure Cache for Redis with Managed Identity. Use when migrating in-memory cache, MemoryCache, System.Runtime.Caching, Redis with password auth, StackExchange.Redis, ServiceStack.Redis, or local Redis to Azure Cache for Redis with DefaultAzureCredential authentication.
---

# Azure Cache for Redis with DefaultAzureCredential

Complete production-ready implementation for migrating .NET applications to use Azure Cache for Redis with DefaultAzureCredential (Managed Identity) authentication. **Microsoft.Azure.StackExchangeRedis 3.3.0+ provides built-in automatic token refresh**, eliminating the need for custom token management logic.

## Dependencies (Modern .NET / SDK-style projects)

```xml
<PackageReference Include="StackExchange.Redis" Version="2.8.16" />
<PackageReference Include="Azure.Identity" Version="1.14.0" />
<PackageReference Include="Microsoft.Azure.StackExchangeRedis" Version="3.3.0" />
```

## Dependencies (Legacy .NET Framework using packages.config)

Target .NET Framework 4.7.2 or 4.8 for best compatibility with netstandard2.0-based libraries. When using packages.config, install packages via NuGet (Package Manager UI/Console) so required transitive dependencies (e.g., Azure.Core, System.Text.Json, etc.) are added automatically.

Minimal packages.config example (top-level packages):

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="StackExchange.Redis" version="2.8.16" targetFramework="net472" />
  <package id="Azure.Identity" version="1.14.0" targetFramework="net472" />
  <package id="Microsoft.Azure.StackExchangeRedis" version="3.3.0" targetFramework="net472" />
</packages>
```

## Configuration

**appsettings.json:**
```json
{
  "Redis": {
    "HostName": "mycachename.redis.cache.windows.net"
  }
}
```

## Production Implementation

**Microsoft.Azure.StackExchangeRedis 3.3.0+ automatically handles token refresh** with sensible defaults:

```csharp
using Azure.Identity;
using Microsoft.Azure.StackExchangeRedis;
using StackExchange.Redis;
using System.Text.Json;

// Redis Connection Service with Built-in Auto Token Refresh
public class RedisConnectionService : IDisposable
{
    private readonly IConfiguration _configuration;
    private ConnectionMultiplexer? _connectionMultiplexer;
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);

    public RedisConnectionService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IDatabase> GetDatabaseAsync()
    {
        if (_connectionMultiplexer == null || !_connectionMultiplexer.IsConnected)
        {
            await _connectionSemaphore.WaitAsync();
            try
            {
                if (_connectionMultiplexer == null || !_connectionMultiplexer.IsConnected)
                {
                    await InitializeConnectionAsync();
                }
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }

        return _connectionMultiplexer!.GetDatabase();
    }

    private async Task InitializeConnectionAsync()
    {
        var hostName = _configuration["Redis:HostName"];

        // Automatic token refresh is enabled by default
        var azureCacheOptions = new AzureCacheOptions
        {
            TokenCredential = new DefaultAzureCredential()
        };

        var configurationOptions = await ConfigurationOptions
            .Parse($"{hostName}:6380")
            .ConfigureForAzureAsync(azureCacheOptions);

        configurationOptions.AbortOnConnectFail = false; // Critical for production resilience

        var oldConnection = _connectionMultiplexer;
        _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(configurationOptions);
        oldConnection?.Dispose();
    }

    public void Dispose()
    {
        _connectionMultiplexer?.Dispose();
        _connectionSemaphore?.Dispose();
    }
}
```

## Key Features of Microsoft.Azure.StackExchangeRedis 3.3.0

- **Automatic Token Refresh**: Tokens are automatically refreshed before expiration without manual intervention
- **Built-in Reconnection Logic**: Handles reauthentication when connections are lost
- **Managed Identity Support**: Full support for DefaultAzureCredential (Managed Identity, Azure CLI, Visual Studio, etc.)
- **Production-Ready Defaults**: Sensible default behavior for token refresh timing and error handling

## Registration in Dependency Injection

```csharp
// ASP.NET Core / Modern .NET
services.AddSingleton<RedisConnectionService>();

// .NET Framework with DI container (e.g., Autofac, Unity)
builder.RegisterType<RedisConnectionService>().As<RedisConnectionService>().SingleInstance();
```

## Usage Example

```csharp
public class CacheService
{
    private readonly RedisConnectionService _redisConnection;

    public CacheService(RedisConnectionService redisConnection)
    {
        _redisConnection = redisConnection;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var db = await _redisConnection.GetDatabaseAsync();
        var value = await db.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var db = await _redisConnection.GetDatabaseAsync();
        var serialized = JsonSerializer.Serialize(value);
        await db.StringSetAsync(key, serialized, expiry);
    }
}
```

## Migration Notes

- **No manual token refresh code needed**: The library handles all token lifecycle management automatically
- **Default refresh timing**: Tokens are refreshed proactively before expiration (typically at ~80% of token lifespan)
- **Graceful failure handling**: By default, token refresh failures don't throw exceptions, allowing the application to continue with existing tokens
