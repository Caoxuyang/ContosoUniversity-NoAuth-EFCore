---
name: dotnet-azure-confluent-kafka
description: Migrate Kafka to Confluent Cloud on Azure with Managed Identity. Use when migrating .NET applications using Confluent.Kafka from local Kafka to Confluent Cloud on Azure, using Azure Managed Identity and DefaultAzureCredential.
---

# Azure Confluent Kafka

Contains code samples, configuration changes, and authentication guidance for migrating .NET applications using `Confluent.Kafka` from local Kafka to **Confluent Cloud on Azure**, using Azure Managed Identity and `DefaultAzureCredential`.

**NOTE**:
- Set up both producer and consumer with OAuthBearer authentication mechanism and Oidc method, and the necessary Oidc parameters.
- Create the `OAuthBearerTokenRefreshCallback` method that uses `DefaultAzureCredential` to acquire OAuth tokens from Azure AD automatically.
- Configure both producer and consumer builders with the token refresh callback.

## Import Dependencies for Azure Managed Identity

NuGet Packages:

```xml
<PackageReference Include="Azure.Identity" Version="1.14.1" />
```

---

## Client Configuration Updates

Replace local Kafka settings with Confluent Cloud values and Azure AD authentication:

```csharp
// Producer Configuration
var producerConfig = new ProducerConfig
{
    BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVER"),
    SecurityProtocol = SecurityProtocol.SaslSsl,
    SaslMechanism = SaslMechanism.OAuthBearer,
    SaslOauthbearerMethod = SaslOauthbearerMethod.Oidc,
    SaslOauthbearerClientId = "ignored", // Not used but required for config
    SaslOauthbearerClientSecret = "ignored", // Not used but required for config
    SaslOauthbearerTokenEndpointUrl = $"http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource='{Environment.GetEnvironmentVariable("KAFKA_SCOPE")}'",
    SaslOauthbearerConfig = $"extension_logicalCluster='{Environment.GetEnvironmentVariable("KAFKA_LOGICAL_CLUSTER_ID")}' extension_identityPoolId='{Environment.GetEnvironmentVariable("KAFKA_IDENTITY_POOL_ID")}'"
};


// Consumer Configuration
var consumerConfig = new ConsumerConfig
{
    BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVER"),
    SecurityProtocol = SecurityProtocol.SaslSsl,
    SaslMechanism = SaslMechanism.OAuthBearer,
    SaslOauthbearerMethod = SaslOauthbearerMethod.Oidc,
    SaslOauthbearerClientId = "ignored", // Not used but required for config
    SaslOauthbearerClientSecret = "ignored", // Not used but required for config
    SaslOauthbearerTokenEndpointUrl = $"http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource='{Environment.GetEnvironmentVariable("KAFKA_SCOPE")}'",
    SaslOauthbearerConfig = $"extension_logicalCluster='{Environment.GetEnvironmentVariable("KAFKA_LOGICAL_CLUSTER_ID")}' extension_identityPoolId='{Environment.GetEnvironmentVariable("KAFKA_IDENTITY_POOL_ID")}'"
};
```

**NOTE**:
- For some ASP.NET Core applications that don't configure Kafka via code but use `appsettings.json`, then modify their `appsettings.json` file to include the above necessary configuration settings for Confluent Cloud.
- Replace placeholders with environment variables or actual values based on your environment.

## Implement Azure Managed Identity Authentication

Use Azure.Core and Azure.Identity namespace and define a method in the same class where the `ProducerConfig` and `ConsumerConfig` are defined to use DefaultAzureCredential for token acquisition:

```csharp
using Azure.Core;
using Azure.Identity;

/// <summary>
/// OAuth Bearer token refresh callback used by Kafka client to authenticate with Confluent Cloud
/// </summary>
private static void OAuthBearerTokenRefreshCallback(IClient client, string config)
{
    try
    {
        // Use the DefaultAzureCredential which will try multiple authentication methods including ManagedIdentity
        var credential = new DefaultAzureCredential();

        // Get token using Azure Identity - note this is using Task.Run to run async code in sync context
        var tokenRequestContext = new TokenRequestContext(new[] { Environment.GetEnvironmentVariable("KAFKA_SCOPE") });
        var accessTokenTask = credential.GetTokenAsync(tokenRequestContext);
        AccessToken accessTokenResponse = accessTokenTask.AsTask().Result;

        // Create token value and expiration for Kafka client
        var tokenValue = accessTokenResponse.Token;
        var expirationMs = DateTimeOffset.UtcNow.AddMinutes(55).ToUnixTimeMilliseconds(); // Set expiry to 5 mins before actual expiry

        Console.WriteLine("Token obtained successfully. Setting on Kafka client...");

        // Create extensions dictionary with the required parameters
        var extensions = new Dictionary<string, string>
        {
            {"logicalCluster", LogicalClusterId},
            {"identityPoolId", IdentityPoolId}
        };

        // Set the token in the Kafka client with extensions
        client.OAuthBearerSetToken(tokenValue, expirationMs, "bearer", extensions);

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
