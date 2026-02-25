---
name: dotnet-azure-database-postgresql
description: Migrate database connections to Azure Database for PostgreSQL with Managed Identity. Use when migrating from PostgreSQL with username/password, Npgsql client, Entity Framework Core with PostgreSQL, Oracle database migration, or local PostgreSQL to Azure PostgreSQL with password-less authentication.
---

# Azure Database for PostgreSQL

Knowledge for migrating .NET applications to use Azure Database for PostgreSQL with password-less solution.

**Important**

- Check the `dotnet-dependency-management` skill to know how to process .csproj file and manage dependencies.
- Update the project to use password-less solution.
- Check if the project is using Entity Framework Core, follow the specific instructions for EF Core or non-EF Core applications.

## Guide

### Azure Database for PostgreSQL with Managed Identity

1. Add the Azure Managed Identity Dependencies

   ```xml
   <PackageReference Include="Azure.Identity" Version="1.14.0" />
   ```

2. Implement token refresh mechanism for Managed Identity authentication

   Azure AD tokens expire after approximately 1 hour. For long-running applications, implement a token provider that refreshes tokens before they expire:

   ```csharp
   using Azure.Core;
   using Azure.Identity;

   public class AzurePostgreSqlTokenProvider
   {
       private readonly TokenCredential _credential;
       private readonly string[] _scopes = new[] { "https://ossrdbms-aad.database.windows.net/.default" };
       private AccessToken _currentToken;
       private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

       public AzurePostgreSqlTokenProvider()
       {
           _credential = new DefaultAzureCredential();
       }

       public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
       {
           // Refresh if token is expired or will expire in the next 5 minutes
           if (_currentToken.ExpiresOn <= DateTimeOffset.UtcNow.AddMinutes(5))
           {
               await _refreshLock.WaitAsync(cancellationToken);
               try
               {
                   // Double-check after acquiring lock
                   if (_currentToken.ExpiresOn <= DateTimeOffset.UtcNow.AddMinutes(5))
                   {
                       _currentToken = await _credential.GetTokenAsync(
                           new TokenRequestContext(_scopes), cancellationToken);
                   }
               }
               finally
               {
                   _refreshLock.Release();
               }
           }

           return _currentToken.Token;
       }
   }
   ```

3. Use the token provider to get fresh tokens when creating connections

   ```csharp
   var ServerName = "<server-name>";
   var DatabaseName = "<database-name>";
   var UserId = "<user-id>";
   var tokenProvider = new AzurePostgreSqlTokenProvider();

   // Get a fresh token for each new connection
   var accessToken = await tokenProvider.GetAccessTokenAsync();
   var connectionString = $"Server={ServerName}.postgres.database.azure.com;Database={DatabaseName};User Id={UserId};Password={accessToken};Ssl Mode=Require;";
   ```

   NOTE: Use `Server=<url>` in connection string. Always obtain a fresh token before creating a new database connection.

### Non-Entity Framework Core .NET Application

1. Add the Npgsql Dependency

   ```xml
   <PackageReference Include="Npgsql" Version="9.0.3" />
   ```

2. Connect to Database with token refresh support

   ```csharp
   var tokenProvider = new AzurePostgreSqlTokenProvider();

   // Get a fresh token before creating connection
   var accessToken = await tokenProvider.GetAccessTokenAsync();
   var connectionString = $"Server={ServerName}.postgres.database.azure.com;Database={DatabaseName};User Id={UserId};Password={accessToken};Ssl Mode=Require;";

   using var connection = new NpgsqlConnection(connectionString);
   await connection.OpenAsync();

   using var command = new NpgsqlCommand("SELECT version();", connection);
   using var reader = await command.ExecuteReaderAsync();
   while (await reader.ReadAsync())
   {
       Console.WriteLine($"PostgreSQL version: {reader.GetString(0)}");
   }
   ```

### Entity Framework Core .NET Application

1. Add Entity Framework Core and PostgreSQL Dependencies

   ```xml
   <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.3" />
   ```

2. Define DbContext with token refresh support

   ```csharp
   public class ApplicationDbContext : DbContext
   {
       private readonly AzurePostgreSqlTokenProvider _tokenProvider;
       private readonly string _serverName;
       private readonly string _databaseName;
       private readonly string _userId;

       public DbSet<DataItem> DataItems { get; set; }

       public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
           AzurePostgreSqlTokenProvider tokenProvider,
           IConfiguration configuration) : base(options)
       {
           _tokenProvider = tokenProvider;
           _serverName = configuration["PostgreSql:ServerName"];
           _databaseName = configuration["PostgreSql:DatabaseName"];
           _userId = configuration["PostgreSql:UserId"];
       }

       protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
       {
           if (!optionsBuilder.IsConfigured)
           {
               var accessToken = _tokenProvider.GetAccessTokenAsync().GetAwaiter().GetResult();
               var connectionString = $"Server={_serverName}.postgres.database.azure.com;Database={_databaseName};User Id={_userId};Password={accessToken};Ssl Mode=Require;";
               optionsBuilder.UseNpgsql(connectionString);
           }
       }
   }
   ```

   Register the token provider as a singleton in your dependency injection container:

   ```csharp
   services.AddSingleton<AzurePostgreSqlTokenProvider>();
   services.AddDbContext<ApplicationDbContext>();
   ```

### Oracle to PostgreSQL Schema Migration Notes

When migrating from Oracle to PostgreSQL:

- Use lowercase for identifiers (table and column names) and data types
- Replace Oracle-specific data types: NUMBER->INTEGER, VARCHAR2->VARCHAR, CLOB->TEXT, BLOB->BYTEA
- Remove `FROM DUAL` in SELECT statements
- Replace Oracle MERGE with PostgreSQL `INSERT ... ON CONFLICT`
- Replace CONNECT BY hierarchical queries with recursive CTEs
- Replace ROWNUM with LIMIT/OFFSET
- Convert Oracle sequences syntax: `EMPLOYEES_SEQ.NEXTVAL` -> `nextval('employees_seq')`
- Convert PL/SQL procedures/functions to PL/pgSQL with dollar-quoted bodies
- Split Oracle packages into separate standalone functions and procedures
- Remove Oracle-specific storage clauses and tablespace specifications
