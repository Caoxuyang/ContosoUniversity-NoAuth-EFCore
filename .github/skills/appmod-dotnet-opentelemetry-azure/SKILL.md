---
name: dotnet-opentelemetry-azure
description: Migrate logging and observability to OpenTelemetry on Azure. Use when migrating from Application Insights SDK, System.Diagnostics, log4net, NLog, Serilog, custom telemetry, APM tools (AppDynamics, Dynatrace, New Relic) to OpenTelemetry with Azure Monitor integration.
---

# OpenTelemetry on Azure

Implement observability in .NET applications using OpenTelemetry with Azure Monitor integration. OpenTelemetry provides vendor-neutral instrumentation for distributed tracing, metrics, and logging.

## Dependencies

```xml
<PackageReference Include="Azure.Monitor.OpenTelemetry.AspNetCore" Version="1.3.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.10.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.10.1" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.10.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.SqlClient" Version="1.10.0-beta.1" />
```

## ASP.NET Core Configuration

### Basic Setup with Azure Monitor

```csharp
using Azure.Monitor.OpenTelemetry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry with Azure Monitor
builder.Services.AddOpenTelemetry()
    .UseAzureMonitor(options =>
    {
        options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    });

var app = builder.Build();
```

### Configuration (appsettings.json)

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-key;IngestionEndpoint=https://region.in.applicationinsights.azure.com/"
  }
}
```

## Advanced Configuration

### Custom Instrumentation

```csharp
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "MyApplication",
            serviceVersion: "1.0.0",
            serviceInstanceId: Environment.MachineName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Filter = httpContext =>
                !httpContext.Request.Path.StartsWithSegments("/health");
        })
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.RecordException = true;
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation())
    .UseAzureMonitor();

var app = builder.Build();
```

### Custom Spans and Activities

```csharp
using System.Diagnostics;

public class OrderService
{
    private static readonly ActivitySource ActivitySource = new("MyApplication.OrderService");

    public async Task<Order> ProcessOrderAsync(OrderRequest request)
    {
        using var activity = ActivitySource.StartActivity("ProcessOrder");

        activity?.SetTag("order.id", request.OrderId);
        activity?.SetTag("order.customer_id", request.CustomerId);
        activity?.SetTag("order.item_count", request.Items.Count);

        try
        {
            // Process the order
            var order = await CreateOrderAsync(request);

            activity?.SetTag("order.total", order.Total);
            activity?.SetStatus(ActivityStatusCode.Ok);

            return order;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}
```

### Custom Metrics

```csharp
using System.Diagnostics.Metrics;

public class OrderMetrics
{
    private readonly Counter<long> _ordersCreated;
    private readonly Histogram<double> _orderProcessingDuration;
    private readonly ObservableGauge<int> _activeOrders;
    private int _activeOrderCount;

    public OrderMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("MyApplication.Orders");

        _ordersCreated = meter.CreateCounter<long>(
            "orders.created",
            unit: "{orders}",
            description: "Number of orders created");

        _orderProcessingDuration = meter.CreateHistogram<double>(
            "orders.processing_duration",
            unit: "ms",
            description: "Order processing duration in milliseconds");

        _activeOrders = meter.CreateObservableGauge(
            "orders.active",
            () => _activeOrderCount,
            unit: "{orders}",
            description: "Number of orders currently being processed");
    }

    public void RecordOrderCreated(string region, string paymentMethod)
    {
        _ordersCreated.Add(1,
            new KeyValuePair<string, object?>("region", region),
            new KeyValuePair<string, object?>("payment_method", paymentMethod));
    }
}

// Registration
builder.Services.AddSingleton<OrderMetrics>();
```

## Logging Integration

### Structured Logging with OpenTelemetry

```csharp
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    options.ParseStateValues = true;
});
```

## .NET Framework Configuration

### Console/Windows Service Applications

```csharp
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService("MyFrameworkApp"))
    .AddHttpClientInstrumentation()
    .AddSqlClientInstrumentation()
    .AddAzureMonitorTraceExporter(options =>
    {
        options.ConnectionString = ConfigurationManager.AppSettings["ApplicationInsights:ConnectionString"];
    })
    .Build();

// Dispose when application shuts down
tracerProvider?.Dispose();
```

## Migration from Application Insights SDK

If migrating from the classic Application Insights SDK:

1. Remove old packages:
   - `Microsoft.ApplicationInsights.AspNetCore`
   - `Microsoft.ApplicationInsights.WorkerService`

2. Add OpenTelemetry packages (listed above)

3. Replace `TelemetryClient` usage with OpenTelemetry patterns:
   ```csharp
   // Before (Application Insights SDK)
   _telemetryClient.TrackEvent("OrderCreated", properties);

   // After (OpenTelemetry)
   using var activity = ActivitySource.StartActivity("OrderCreated");
   activity?.SetTag("property_name", value);
   ```

4. Update custom metrics:
   ```csharp
   // Before
   _telemetryClient.GetMetric("OrderCount").TrackValue(1);

   // After
   _ordersCounter.Add(1);
   ```
