---
name: dotnet-azure-communication-email
description: Migrate email sending to Azure Communication Services Email with Managed Identity. Use when migrating from SMTP, SendGrid, custom email services, or legacy email configurations using connection strings to Azure Communication Services with DefaultAzureCredential.
---

# Azure Communication Services email

Contains samples, instructions, and other information that is useful for generating code that uses Azure Communication Services email.

**NOTE**:
- Use Azure Managed Identity solution for Azure Communication Services email connection, **DO NOT** use connection string.
- Use EmailClient is preferred.
- Refer to the samples in the knowledge when generating new Azure Communication code.

## Sample code for sending email using Azure Communication Services

### Add Dependencies

```xml
<PackageReference Include="Azure.Communication.Email" Version="1.0.1" />
<PackageReference Include="Azure.Identity" Version="1.14.0" />
```

### Init SMTP client

```csharp
string endpoint = Configuration["AzureCommunicationEmail:EndpointUrl"]

var credential = new DefaultAzureCredential();

var emailClient = new EmailClient(new Uri(endpoint), credential);
```

### Send Email with synchronous status polling

```csharp
string sender = "donotreply@xxxx.azurecomm.net";
string recipient = "emailalias@contoso.com";
string subject = "<Mail title>";
string htmlContent = "<Mail body>";

EmailSendOperation emailSendOperation = await emailClient.SendAsync(
    Azure.WaitUntil.Completed,
    sender,
    recipient,
    subject,
    htmlContent);
EmailSendResult statusMonitor = emailSendOperation.Value;
Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");
```

### Send Email with asynchronous status polling

```csharp
string sender = "donotreply@xxxx.azurecomm.net";
string recipient = "emailalias@contoso.com";
string subject = "<Mail title>";
string htmlContent = "<Mail body>";

EmailSendOperation emailSendOperation = await emailClient.SendAsync(
    Azure.WaitUntil.Started,
    sender,
    recipient,
    subject,
    htmlContent);

while (true)
{
    await emailSendOperation.UpdateStatusAsync();
    if (emailSendOperation.HasCompleted)
    {
        break;
    }
    await Task.Delay(100);
}

if (emailSendOperation.HasValue)
{
    Console.WriteLine($"Email queued for delivery. Status = {emailSendOperation.Value.Status}");
}
```
