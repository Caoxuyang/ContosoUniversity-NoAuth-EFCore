---
name: dotnet-azure-sql-database
description: Migrate database connections to Azure SQL Database or Azure SQL Managed Instance with Managed Identity. Use when migrating from SQL Server with username/password, System.Data.SqlClient, Entity Framework 6, or local SQL Server to Azure SQL with DefaultAzureCredential authentication.
---

# Azure SQL Database and Managed Instance

Knowledge for migrating .NET applications to use Azure SQL Database and Azure SQL Managed Instance with Azure Managed Identity.

NOTE:

* Use Azure Managed Identity solution for Azure SQL Database and Managed Instance connections.
* Upgrade SDK `System.Data.SqlClient` to `Microsoft.Data.SqlClient`.

Guide:

1. Add the Azure SQL Database and Managed Identity Dependencies, also, these package should also be referenced in places like web.config, app.config, etc.

   * `<package id="Azure.Identity" version="1.14.0" />`
   * `<package id="Microsoft.Data.SqlClient" version="6.0.2" />`

1. Replace the `System.Data.SqlClient` package with `Microsoft.Data.SqlClient`.

   No direct classes or methods replacements needed as `Microsoft.Data.SqlClient` maintains API compatibility with `System.Data.SqlClient`.

1. **DO** use managed identity for Azure SQL Database connection.

   Replace the user & password in the connection string with Azure Managed Identity style `Authentication=Active Directory Default;`.
   Find all the connection strings in config files and code, and replace them. Typically, local sql server connection strings should contain "(localdb)".

   Configuration example:

   ```json
   {
       "ConnectionStrings": {
           "DefaultConnection": "Server=tcp:<server-name>.database.windows.net;Database=<database-name>;Authentication=Active Directory Default;TrustServerCertificate=True"
       }
   }
   ```

1. **DO** use managed identity for Azure SQL Managed Instance connection.

   Replace the user & password in the connection string with Azure Managed Identity style `Authentication=Active Directory Default;`. Note the port and FQDN format differences.

   Configuration example:

   ```json
   {
       "ConnectionStrings": {
           "ManagedInstanceConnection": "Server=tcp:<managed-instance-name>.<dns-zone>.database.windows.net,3342;Database=<database-name>;Authentication=Active Directory Default;TrustServerCertificate=True"
       }
   }
   ```

## Connection String Formats

### Azure SQL Database
```
Server=tcp:<server-name>.database.windows.net;Database=<database-name>;Authentication=Active Directory Default;
```

### Azure SQL Managed Instance
```
Server=tcp:<managed-instance-name>.<dns-zone>.database.windows.net,3342;Database=<database-name>;Authentication=Active Directory Default;
```

Key differences:
- Managed Instance uses port 3342 (public endpoint)
- FQDN includes DNS zone: `<instance-name>.<dns-zone>.database.windows.net`
- Private endpoint omits port: `<instance-name>.<dns-zone>.database.windows.net`

---

## Entity Framework 6 SQL Server provider based on Microsoft.Data.SqlClient

This Entity Framework 6 provider is a replacement provider for the built-in SQL Server provider.

This provider depends on the modern [Microsoft.Data.SqlClient](https://github.com/dotnet/SqlClient) ADO.NET provider, which includes the following advantages over the currently used driver:

- Current client receiving full support in contrast to `System.Data.SqlClient`, which is in maintenance mode
- Supports new SQL Server features, including support for the SQL Server 2022 enhanced client protocol (TDS8)
- Supports most Azure Active Directory authentication methods
- Supports Always Encrypted with .NET

## Configuration

There are various ways to configure Entity Framework to use this provider.

You can register the provider in code using an attribute:

```csharp
[DbConfigurationType(typeof(MicrosoftSqlDbConfiguration))]
public class SchoolContext : DbContext
{
    public SchoolContext() : base()
    {
    }

    public DbSet<Student> Students { get; set; }
}
```

If you have multiple classes inheriting from DbContext in your solution, add the DbConfigurationType attribute to all of them.

Or you can use the SetConfiguration method before any data access calls:

```csharp
 DbConfiguration.SetConfiguration(new MicrosoftSqlDbConfiguration());
```

Or add the following lines to your existing derived DbConfiguration class:

```csharp
SetProviderFactory(MicrosoftSqlProviderServices.ProviderInvariantName, Microsoft.Data.SqlClient.SqlClientFactory.Instance);
SetProviderServices(MicrosoftSqlProviderServices.ProviderInvariantName, MicrosoftSqlProviderServices.Instance);
// Optional
SetExecutionStrategy(MicrosoftSqlProviderServices.ProviderInvariantName, () => new MicrosoftSqlAzureExecutionStrategy());
```

You can also use App.Config based configuration:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <configSections>
        <section name="entityFramework" type="System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false" />
    </configSections>
    <entityFramework>
        <providers>
            <provider invariantName="Microsoft.Data.SqlClient" type="System.Data.Entity.SqlServer.MicrosoftSqlProviderServices, Microsoft.EntityFramework.SqlServer" />
        </providers>
    </entityFramework>
    <system.data>
        <DbProviderFactories>
           <add name="SqlClient Data Provider"
             invariant="Microsoft.Data.SqlClient"
             description=".NET Framework Data Provider for SqlServer"
             type="Microsoft.Data.SqlClient.SqlClientFactory, Microsoft.Data.SqlClient" />
        </DbProviderFactories>
    </system.data>
</configuration>
```

## SQL Client related code changes

To use the provider in an existing solution, a few code changes are required (as needed).

`using System.Data.SqlClient;` => `using Microsoft.Data.SqlClient;`

`using Microsoft.SqlServer.Server;` => `using Microsoft.Data.SqlClient.Server;`

The following classes have been renamed to avoid conflicts:

`SqlAzureExecutionStrategy` => `MicrosoftSqlAzureExecutionStrategy`

`SqlDbConfiguration` => `MicrosoftSqlDbConfiguration`

`SqlProviderServices` => `MicrosoftSqlProviderServices`

`SqlServerMigrationSqlGenerator` => `MicrosoftSqlServerMigrationSqlGenerator`

`SqlSpatialServices` => `MicrosoftSqlSpatialServices`

`SqlConnectionFactory` => `MicrosoftSqlConnectionFactory`

`LocalDbConnectionFactory` => `MicrosoftLocalDbConnectionFactory`
