---
name: dotnet-azure-servicebus
description: Migrate message queues to Azure Service Bus with Managed Identity. Use when migrating from RabbitMQ, MSMQ, or other message brokers to Azure Service Bus with DefaultAzureCredential authentication. Covers queues, topics, subscriptions, and message processing.
---

# Azure Service Bus

Contains samples, instructions, and other information that is useful for generating code that uses Azure Service Bus.

**NOTE**:
- Use Azure Managed Identity solution for Azure Service Bus connection, **DO NOT** use connection string.
- Refer to the samples in the knowledge when generate new service bus code.

## Collect project context

Before migration, analyze the original messaging resource topology and map to the right service bus topology.

Collect some project context:
- The service bus resource names.

**Note**:
- Create service bus subscription for entity type topic, **DO NOT** create service bus queue with same subscription name.
- Avoid new object name conflict with existing object name.

## Import Dependencies for Azure Service Bus and Azure Managed Identity

### Add the Azure Service Bus Dependencies

NuGet Packages:

```xml
<PackageReference Include="Azure.Messaging.ServiceBus" Version="7.19.0" />
<PackageReference Include="Azure.Identity" Version="1.14.0" />
```

---

## Use Azure Managed Identity for App to Connect to Azure Service Bus

### Add Service Bus Settings

```json
{
  "AzureServiceBus": {
    "FullyQualifiedNamespace": "${SERVICE_BUS_NAMESPACE}.servicebus.windows.net"
  }
}
```

**NOTE**:
- For ASP.NET Core applications, add this to your `appsettings.json` file.
- Replace placeholders with environment variables or actual values based on your environment.

### Initialize Service Bus client

```csharp
var namespace = Configuration["AzureServiceBus:FullyQualifiedNamespace"];
var credential = new DefaultAzureCredential();

var serviceBusClient = new ServiceBusClient(
    namespace,
    credential);
```

**Note**: We use Azure Managed Identity for Azure Service Bus, do not use connection string to initialize clients.

---

## Send Azure Service Bus message

When constructing the service bus message, check if there is a routing key in the context. If a routing key is present, set it as the message's Subject property. If there is no routing key, do not set the Subject property.

- With Routing key

```csharp
var sender = serviceBusClient.CreateSender("topicName");

var message = new ServiceBusMessage(body)
{
    Subject = "routing.key"
};

await sender.SendMessageAsync(message);
```

- Without Routing key

```csharp
var sender = serviceBusClient.CreateSender("queueName");

var message = new ServiceBusMessage(body);

await sender.SendMessageAsync(message);
```

---

## Receive Azure Service Bus Message

### Create receiver

```csharp
// For queue
var receiver = serviceBusClient.CreateReceiver(queueName);

// For topic + subscription
var receiver = serviceBusClient.CreateReceiver(topicName, subscriptionName);
```

### Receive message
```csharp
// receive message
var receivedMessage = await receiver.ReceiveMessageAsync();
var body = receivedMessage.Body.ToString();
```

## Process Azure Service Bus Message

### Configure Processor

```csharp
// For Queue:
var processor = serviceBusClient.CreateProcessor(queueName);

// For Topic/Subscription:
var processor = serviceBusClient.CreateProcessor(topicName, subscriptionName);
```

### Message Handler Implementation

```csharp
// Simple message handler for processing messages
async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();

    try
    {
        // Process the message
        Console.WriteLine($"Received message: {body}");

        // Complete the message
        await args.CompleteMessageAsync(args.Message);
    }
    catch (Exception ex)
    {
        // Handle any exceptions during processing
        Console.WriteLine($"Error processing message: {ex.Message}");

        // Abandon the message to make it available for processing again
        await args.AbandonMessageAsync(args.Message);
    }
}

// Error handler
Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine($"Error occurred: {args.Exception.Message}");
    return Task.CompletedTask;
}
```

### Register Message Handlers

```csharp
// Register message handlers
processor.ProcessMessageAsync += MessageHandler;
processor.ProcessErrorAsync += ErrorHandler;

// Start processing
await processor.StartProcessingAsync();
```
---

## Azure Service Bus Sample Code

### Create Azure Service Bus Topic and Subscription with Rule Options

```csharp
public async Task<TopicProperties> CreateTopicIfNotExistsAsync(string topicName)
{
    try
    {
        return await serviceBusClient.GetTopicAsync(topicName);
    }
    catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
    {
        return await serviceBusClient.CreateTopicAsync(topicName);
    }
}

public async Task<SubscriptionProperties> CreateSubscriptionIfNotExistAsync(
    string topicName,
    string subscriptionName,
    string routeKey)
{
    try
    {
        return await serviceBusClient.GetSubscriptionAsync(topicName, subscriptionName);
    }
    catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
    {
        // Create a correlation filter with label as the route key
        var subOptions = new CreateSubscriptionOptions(topicName, subscriptionName);
        var ruleOptions = new CreateRuleOptions{
            Name = "CorrelationRule",
            Filter = new CorrelationRuleFilter { Subject = "Key", CorrelationId = routeKey }
        };

        var subscription = await serviceBusClient.CreateSubscriptionAsync(subOptions, ruleOptions);

        return subscription;
    }
}
```

### Create Queue in Azure Service Bus

```csharp
public async Task<QueueProperties> CreateQueueIfNotExistsAsync(string queueName)
{
    try
    {
        return await _adminClient.GetQueueAsync(queueName);
    }
    catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
    {
        return await _adminClient.CreateQueueAsync(queueName);
    }
}
```

### Service Bus Processor with Session Support

```csharp
// For session-enabled queues/topics
ServiceBusSessionProcessor sessionProcessor = serviceBusClient.CreateSessionProcessor(queueName);

// Register session message handlers
sessionProcessor.ProcessMessageAsync += SessionMessageHandler;
sessionProcessor.ProcessErrorAsync += SessionErrorHandler;

// Start processing
await sessionProcessor.StartProcessingAsync();

// Session message handler
async Task SessionMessageHandler(ProcessSessionMessageEventArgs args)
{
    string sessionId = args.Message.SessionId;
    string body = args.Message.Body.ToString();

    try
    {
        // Process the message
        Console.WriteLine($"Received message from session {sessionId}: {body}");

        // Complete the message
        await args.CompleteMessageAsync(args.Message);
    }
    catch (Exception ex)
    {
        // Handle any exceptions during processing
        Console.WriteLine($"Error processing message: {ex.Message}");
        await args.AbandonMessageAsync(args.Message);
    }
}

// Session error handler
Task SessionErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine($"Error occurred: {args.Exception.Message}");
    return Task.CompletedTask;
}
```
