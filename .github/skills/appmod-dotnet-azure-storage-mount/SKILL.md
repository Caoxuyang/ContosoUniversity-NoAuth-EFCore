---
name: dotnet-azure-storage-mount
description: Migrate local file I/O to Azure Storage mount paths. Use when migrating from hard-coded local file paths (Windows C:\, Linux /home/) to Azure mounted storage paths while maintaining the same file operations functionality.
---

# Azure Storage Mount

## Overview

Migrate a .Net source code from reading from or writing to hard-coded local file paths to an Azure mounted storage path while maintaining the same functionality.

## Identification Guidelines

Use standard regular expressions to identify local file path patterns. Then, analyze each matched string to verify whether it represents a valid file path:

- Look for path patterns: Windows paths (C:\path\to\file), Unix paths (/home/user/file), or relative paths (./file or ../file). Refer to the examples below.
- Consider the context: In property files, not all strings with slashes are file paths.
- DO NOT modify URLs, database url, connection string, username, password, package names, or user identifiers that like a local file path.

### Examples of local file paths:

- Windows style local file paths:
  - C:\app.log
  - C:\Users\someone\logs\app.log
  - C:\Program Files\MyApp\data\input.csv
  - D:\Projects\MyProject\config.xml
  - H:\data
  - File path with system or user environment variables:
    - "%ALLUSERSPROFILE%",
    - "%APPDATA%",
    - "%CD%",
    - "%COMPUTERNAME%",
    - "%HOMEDRIVE%",
    - "%HOMEPATH%",
    - "%HOMESHARE%",
    - "%LOCALAPPDATA%",
    - "%OneDrive%",
    - "%ProgramData%",
    - "%ProgramFiles%",
    - "%ProgramFiles(x86)%",
    - "%ProgramW6432%",
    - "%SYSTEMDRIVE%",
    - "%SYSTEMROOT%",
    - "%TEMP%",
    - "%TMP%",
    - "%USERPROFILE%",
    - "%WINDIR%"

- Linux style local file paths:
  - /etc/config.xml
  - /usr/local/bin/script.sh
  - /home/user/data/input.csv
  - ~/apps/config.json
  - Some environment variables:
    - "$HOME",
    - "$XDG_CONFIG_HOME",
    - "$XDG_DATA_HOME"

### Examples of non-local-file paths (IMPORTANT: DON'T CHANGE THEM):

- spring.database.username="/home/test"
- spring.database.url=jdbc:mysql://home/user/app:user@127.0.0.1:3306/database_name ("home/user/app" is not path, it's username in the database url.)
- spring.datasource.schema=classpath:schema.sql
- class: com.example.package/class
- url: http://localhost:8080/api

### Path Transformation Rules

Follow these steps to properly convert each identified file path:

1. Remove OS-specific prefix:
    - For Windows: Remove drive letter (e.g., C:, C:\Users)
    - For Unix: Remove root directories like /home/<user>, /usr, /Users

2. Extract and preserve the application-specific portion of the path.

3. **DO** in place updates to the application-specific portion of the paths, no helper for file path check required.
   Example: `C:\path\to\file` -> `Path.Combine(AzureMountPath, "path/to/file")`

4. Use azure mount path:
    - Standard format: "/mnt/azure/your-path"
    - For Spring property files (appsettings.json), use "/mnt/azure/your-path" to provide a default value
    - For source code with configuration, read the azure mount path from environment variable `AZURE_MOUNT_PATH`:

      ```csharp
      string AzureMountPath = Environment.GetEnvironmentVariable("AZURE_MOUNT_PATH") ?? "/mnt/azure";
      string filePath = Path.Combine(AzureMountPath, "data/inputs.csv");
      ```

## Examples

- Windows: "C:\Users\someone\logs\app.log" -> "/mnt/azure/logs/app.log"
- Unix: "/etc/config.xml" -> "/mnt/azure/config.xml"
- User directory: "/Users/bob/config.xml" -> "/mnt/azure/config.xml"
- Relative: "./data/info.txt" -> "/mnt/azure/data/info.txt"

## More guides:

- DO NOT modify: class name, method name, field name
- DO NOT modify: commented out code
- DO modify: `File file = new File("C://some-folder/config.xml");`
- DO modify: `File file = new File("/Users/bob/config.xml");`
- DO modify: `File file = new File("/etc/config.xml");`
- DO modify: `Paths.get("C://some-folder/config.xml");`
- DO modify: `Path.of("C://some-folder/config.xml");`
- DO NOT modify: `File file = new File("/WEB-INF/config.xml");`
- DO NOT modify: `templateResolver.setPrefix("/WEB-INF/templates/");`
- DO NOT modify: `/src/main/resources`
