---
name: analyze-dependencies-for-azure
description: Analyze application dependency files to identify dependencies that should be migrated to Azure services, and recommend framework upgrade paths.
---

# Analyze dependencies for Azure service recommendations and upgrade paths

This skill analyzes application dependency and configuration files across
multiple repositories. It processes:

- **Build/project files**: `pom.xml`, `build.gradle`, `build.gradle.kts`,
  `*.csproj`, `*.fsproj`, `*.vbproj`
- **Package manifests**: `packages.config`, `Directory.Build.props`,
  `Directory.Packages.props`
- **Application configuration**: `web.config`, `app.config`,
  `appsettings*.json`, `application*.properties`, `application*.yml/yaml`
- **Gradle metadata**: `settings.gradle`, `settings.gradle.kts`,
  `gradle.properties`
- **Infrastructure descriptors**: `docker-compose.yml/yaml`, `Dockerfile`

It produces two outputs:

1. **Azure service recommendations** — dependencies that should map
   to Azure services when migrating to Azure.
2. **Framework upgrade paths** — detected framework/runtime versions
   and their recommended upgrade targets.

You receive:

- **Dependency file contents** stored as a JSON file at
  `dependency-files-json-path`. Read this file first. It is a JSON
  object whose keys are `"{repoName}::{filePath}"` and values are
  the file contents (text).

## Your task

1. **Read** the JSON file at `dependency-files-json-path`.

2. **Identify Azure-relevant dependencies** — libraries, packages, or
   modules that represent backing services, middleware, or
   infrastructure (databases, messaging, caching, storage,
   communication/RPC, auth, secrets, email, logging/observability,
   scheduling, etc.) and recommend the best Azure service for each.
   Use all available signals:

   - **Package references** in project/build files (`pom.xml`,
     `build.gradle`, `*.csproj`, `packages.config`).
   - **Connection strings** in `web.config`, `app.config`, and
     `appsettings*.json` — look for `<connectionStrings>` sections
     and `"ConnectionStrings"` JSON keys. Identify the backing service
     type (SQL Server, PostgreSQL, MySQL, Redis, Service Bus, etc.)
     from provider names, keywords, and port numbers in connection
     strings.
   - **WCF / SOAP / RPC configuration** in `web.config` and
     `app.config` — look for `<system.serviceModel>` sections with
     `<services>`, `<bindings>`, and `<client>` elements. Also detect
     WCF-related NuGet packages (`System.ServiceModel.*`) in
     `packages.config`. WCF indicates a communication dependency that
     should map to Azure API Management, Azure App Service, or
     CoreWCF on Azure.
   - **Logging and observability libraries** — detect log4net, NLog,
     Serilog, `Microsoft.ApplicationInsights.*`, `System.Diagnostics`
     tracing, and similar packages in `packages.config` or `*.csproj`.
     These should map to Azure Monitor / Application Insights.
   - **Spring Boot configuration** in `application*.properties` and
     `application*.yml/yaml` — look for `spring.datasource.*`,
     `spring.redis.*`, `spring.kafka.*`, `spring.rabbitmq.*`,
     `spring.mail.*`, `spring.data.mongodb.*`, `spring.cache.*`, and
     similar property prefixes to detect databases, caches, and
     message brokers.
   - **Docker Compose services** in `docker-compose.yml/yaml` — map
     container images to infrastructure services (e.g. `postgres` →
     Azure Database for PostgreSQL, `redis` → Azure Cache for Redis,
     `rabbitmq` → Azure Service Bus, `mongo` → Azure Cosmos DB,
     `elasticsearch` → Azure AI Search).
   - **Dockerfile base images** — confirm framework versions and
     runtime environments (e.g. `mcr.microsoft.com/dotnet/aspnet:6.0`
     confirms .NET 6, `openjdk:17` confirms Java 17).

3. **Detect framework / runtime versions** from the dependency files
   and recommend upgrade paths. Report only these framework types:
   - Java X (e.g. "Java 8", "Java 11", "Java 17")
   - Spring Boot X / Spring X (e.g. "Spring Boot 2.7")
   - .NET Framework X (e.g. ".NET Framework 4.8")
   - .NET X (e.g. ".NET 6", ".NET 8")
   - Java EE / Jakarta EE (e.g. "Java EE 8")

   Use all available version sources:
   - `<TargetFramework>` in `*.csproj` files
   - `<TargetFrameworkVersion>` in legacy `*.csproj` files
   - `Directory.Build.props` for centralized version properties
   - Presence of `packages.config` as a signal for legacy .NET Framework
   - `java.sourceCompatibility` or `sourceCompatibility` in `build.gradle`
   - `gradle.properties` for version variables (e.g. `javaVersion=17`)
   - `<maven.compiler.source>` in `pom.xml`
   - `FROM` base images in `Dockerfile` (e.g. `FROM openjdk:17-slim`)

   **Current recommended upgrade targets** (use these when
   recommending upgrade paths):
   - **Java**: Java 21 (LTS, supported until Sep 2028).
     Next LTS: Java 25 (Sep 2025).
   - **Spring Boot**: Spring Boot 3.4.x (latest stable,
     requires Java 17+). Spring Boot 3.x requires Spring
     Framework 6.x and Jakarta EE (javax.* → jakarta.*).
   - **.NET**: .NET 10 (LTS, released Nov 2025).
     .NET 8 LTS reaches end-of-support Nov 2026.
   - **.NET Framework**: .NET Framework 4.8.1 is the final
     version. Recommend migrating to .NET 10 for new
     development.
   - **Java EE / Jakarta EE**: Jakarta EE 10+ (aligned with
     Spring Boot 3.x / Spring Framework 6.x).

   Each Java app must appear in at least a JDK entry. Each .NET app
   must appear in at least a .NET Framework or .NET entry. Add
   Spring Boot / Java EE entries only when detected.

4. **Produce a JSON output file** at the path specified by
   `output-json-path`.

## Output format

Write a JSON file with this exact structure:

```json
{
  "recommendations": [
    {
      "dependency": "<human-readable name, e.g. 'Apache Kafka'>",
      "category": "<database | messaging | caching | storage | communication | auth | secrets | email | logging | scheduling | other>",
      "detectedIn": ["<repoName1>", "<repoName2>"],
      "detectedArtifacts": ["<artifact-id or package-name>"],
      "options": ["<Azure Service Option 1>", "<Azure Service Option 2>"],
      "recommendation": "<ONE recommended Azure service>",
      "rationale": "<1-2 sentences>"
    }
  ],
  "upgradePaths": [
    {
      "framework": "<framework with version, e.g. 'Java 11', '.NET Framework 4.8'>",
      "detectedIn": ["<repoName1>", "<repoName2>"],
      "apps": "<number of apps using this framework version>",
      "recommendation": "<latest LTS stable version to upgrade to>",
      "rationale": "<short rationale>"
    }
  ]
}
```

## Rules

1. **Aggregate across repos.** Same dependency in multiple repos →
   single entry with all repos in `detectedIn`.
2. **Be specific with Azure names.** Use full names like
   "Azure Cache for Redis", "Azure Service Bus".
3. **One entry per logical dependency.** Group related artifacts
   (e.g. `spring-kafka` + `kafka-clients` → one Kafka entry).
4. **Skip irrelevant dependencies.** No test frameworks, build
   plugins, or standard utilities (JUnit, Mockito, Lombok, xUnit,
   Newtonsoft.Json, AutoMapper, etc.). **Do include** logging and
   observability libraries (log4net, NLog, Serilog, Application
   Insights), communication/RPC frameworks (WCF/System.ServiceModel,
   gRPC, SOAP services), and any library that implies a backing
   service — these all have Azure migration implications.
5. **Deduplicate across package references and configuration signals.**
   When the same backing service is detected from both a package
   reference (e.g. `Npgsql` NuGet package) and a configuration signal
   (e.g. a PostgreSQL connection string in `appsettings.json`), produce
   only one recommendation entry. Combine the detection sources in
   `detectedArtifacts`.
6. **Ignore credential values.** Extract only the service type and
   connection metadata. Never include passwords, access keys, SAS
   tokens, or other secrets in the output.
7. **Group upgrade paths by framework + version.** Multiple apps
   on the same version → single entry with total app count and
   all repo names in `detectedIn`.
8. **Always produce valid JSON.** No trailing commas, no comments.
9. **Both arrays are required.** If nothing found, use empty arrays.

## Completion criteria

1. A valid JSON file exists at `output-json-path`.
2. Both `recommendations` and `upgradePaths` arrays are present.
3. Each entry has a non-empty `recommendation` and `rationale`.
4. Each `upgradePaths` entry has a `detectedIn` array listing repos.

If dependency files are empty or cannot be parsed, output
`{"recommendations": [], "upgradePaths": []}`.
