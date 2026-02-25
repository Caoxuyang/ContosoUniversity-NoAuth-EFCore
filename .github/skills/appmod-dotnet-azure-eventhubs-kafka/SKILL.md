---
name: dotnet-azure-eventhubs-kafka
description: Migrate Kafka to Azure Event Hubs for Kafka with Managed Identity. Use when migrating .NET applications using Confluent.Kafka from local Kafka to Azure Event Hubs for Kafka, using Azure Managed Identity and DefaultAzureCredential.
---

# Azure Event Hubs Kafka

Contains code samples, configuration changes, and authentication guidance for migrating .NET applications using `Confluent.Kafka` from local Kafka to **Azure Event Hubs for Kafka**, using Azure Managed Identity and `DefaultAzureCredential`.

**NOTE**:
- Set up both producer and consumer with OAuthBearer authentication mechanism.
- Create the `OAuthBearerTokenRefreshCallback` method that uses `DefaultAzureCredential` to acquire OAuth tokens from Azure AD automatically.
- Configure both producer and consumer builders with the token refresh callback.

## Import Dependencies for Azure Managed Identity

Add the `Azure.Identity` NuGet Packages only, and don't add any Azure Event Hubs packages:

```xml
<PackageReference Include="Azure.Identity" Version="1.14.1" />
```

---

## Client Configuration Updates

Replace local Kafka settings with Event Hubs endpoint and Azure AD authentication:

```csharp
// Producer Configuration
var producerConfig = new ProducerConfig
{
    BootstrapServers = Environment.GetEnvironmentVariable("EVENT_HUBS_ENDPOINT"), // The value should be the Event Hubs namespace with 9093 port, e.g., "myeventhubnamespace.servicebus.windows.net:9093"
    SecurityProtocol = SecurityProtocol.SaslSsl,
    SaslMechanism = SaslMechanism.OAuthBearer,
};


// Consumer Configuration
var consumerConfig = new ConsumerConfig
{
    BootstrapServers = Environment.GetEnvironmentVariable("EVENT_HUBS_ENDPOINT"), // The value should be the Event Hubs namespace with 9093 port, e.g., "myeventhubnamespace.servicebus.windows.net:9093"
    SecurityProtocol = SecurityProtocol.SaslSsl,
    SaslMechanism = SaslMechanism.OAuthBearer,
};
```

**NOTE**:
- For some ASP.NET Core applications that don't configure Kafka via code but use `appsettings.json`, then modify their `appsettings.json` file to include the above necessary configuration settings for Event Hubs.
- Replace placeholders with environment variables or actual values based on your environment.
- Don't configure the `SaslOauthbearerMethod` property as `Oidc` as it is not applicable for Azure Event Hubs for Kafka.

## Implement Azure Managed Identity Authentication

Use Azure.Core and Azure.Identity namespace and define a method in the same class where the `ProducerConfig` and `ConsumerConfig` are defined to use DefaultAzureCredential for token acquisition:

```csharp
using Azure.Core;
using Azure.Identity;

/// <summary>
/// OAuth Bearer token refresh callback used by Kafka client to authenticate with Azure Event Hubs for Kafka
/// </summary>
private static void OAuthBearerTokenRefreshCallback(IClient client, string config)
{
    try
    {
        Console.WriteLine("Requesting new OAuth token using Azure Managed Identity");

        // Use the DefaultAzureCredential which will try multiple authentication methods including ManagedIdentity
        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            // Uncomment the following line if you want to use specific managed identity
            // ManagedIdentityClientId = ClientId
        });

        // Dynamically construct the scope from EVENT_HUBS_ENDPOINT
        var bootstrapServer = Environment.GetEnvironmentVariable("EVENT_HUBS_ENDPOINT");
        var host = bootstrapServer?.Split(':')[0]; // Extract host part before ":"
        var scope = $"https://{host}/.default";

        // Get token using Azure Identity - note this is using Task.Run to run async code in sync context
        var tokenRequestContext = new TokenRequestContext(new[] { scope });
        Console.WriteLine($"Requesting token for scope: {scope}");
        var accessTokenTask = credential.GetTokenAsync(tokenRequestContext);
        AccessToken accessTokenResponse = accessTokenTask.AsTask().Result;

        // Create token value and expiration for Kafka client
        var tokenValue = accessTokenResponse.Token;
        var expirationMs = DateTimeOffset.UtcNow.AddMinutes(55).ToUnixTimeMilliseconds(); // Set expiry to 5 mins before actual expiry

        Console.WriteLine("Token obtained successfully. Setting on Kafka client...");

        client.OAuthBearerSetToken(tokenValue, expirationMs, "bearer");

        Console.WriteLine($"Successfully set OAuth token with extensions. Expires at: {DateTimeOffset.FromUnixTimeMilliseconds(expirationMs)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error obtaining OAuth token: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        client.OAuthBearerSetTokenFailure(ex.ToString());
    }
}

```

Then configure the producer and consumer to use this callback:
```csharp
using var producer = new ProducerBuilder<string, string>(producerConfig)
    .SetOAuthBearerTokenRefreshHandler(OAuthBearerTokenRefreshCallback)
    .Build();

using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
    .SetOAuthBearerTokenRefreshHandler(OAuthBearerTokenRefreshCallback)
    .Build();
```

**NOTE**:
- Don't remove the original code unless it's conflict with the migration plan and need to be replaced.
