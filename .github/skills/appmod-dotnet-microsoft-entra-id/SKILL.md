---
name: dotnet-microsoft-entra-id
description: Migrate .NET app authentication to Microsoft Entra ID (formerly Azure AD). Use when migrating from forms authentication, custom authentication, LDAP, Windows authentication, or implementing OAuth/OIDC for ASP.NET Core, ASP.NET Web Forms, and Microsoft Graph integration.
---

# Microsoft Entra ID

Migrate a .NET source code to use Microsoft Entra ID (formerly Azure Active Directory) authentication.

## ASP.NET Core Web App

### Import Dependencies

```xml
<PackageReference Include="Microsoft.Identity.Web" Version="3.7.1" />
<PackageReference Include="Microsoft.Identity.Web.UI" Version="3.7.1" />
<PackageReference Include="Microsoft.Identity.Web.DownstreamApi" Version="3.7.1" />
<PackageReference Include="Microsoft.Identity.Web.GraphServiceClient" Version="3.7.1" />
```

### Configuration (appsettings.json)

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "yourdomain.onmicrosoft.com",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc"
  }
}
```

### Program.cs Configuration

```csharp
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// Add Microsoft Entra ID authentication
builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
    .AddInMemoryTokenCaches();

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddRazorPages();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
```

### Access User Claims

```csharp
[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst("name")?.Value;

        return View();
    }
}
```

---

## ASP.NET Web Forms (.NET Framework)

### Import Dependencies

```xml
<PackageReference Include="Microsoft.Owin.Host.SystemWeb" Version="4.2.2" />
<PackageReference Include="Microsoft.Owin.Security.OpenIdConnect" Version="4.2.2" />
<PackageReference Include="Microsoft.Owin.Security.Cookies" Version="4.2.2" />
<PackageReference Include="Microsoft.IdentityModel.Protocols.OpenIdConnect" Version="8.2.1" />
```

### Web.config Configuration

```xml
<configuration>
  <appSettings>
    <add key="ida:ClientId" value="your-client-id" />
    <add key="ida:AADInstance" value="https://login.microsoftonline.com/{0}" />
    <add key="ida:Domain" value="yourdomain.onmicrosoft.com" />
    <add key="ida:TenantId" value="your-tenant-id" />
    <add key="ida:PostLogoutRedirectUri" value="https://localhost:44300/" />
  </appSettings>
</configuration>
```

### OWIN Startup Configuration

```csharp
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.Configuration;

[assembly: OwinStartup(typeof(YourApp.Startup))]

namespace YourApp
{
    public class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        private static string authority = string.Format(aadInstance, tenantId);

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = authority,
                    PostLogoutRedirectUri = postLogoutRedirectUri,
                    RedirectUri = postLogoutRedirectUri,
                    Scope = "openid profile email",
                    ResponseType = "id_token",
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        NameClaimType = "name"
                    }
                });
        }
    }
}
```

---

## Microsoft Graph Integration

### Configuration

```json
{
  "MicrosoftGraph": {
    "BaseUrl": "https://graph.microsoft.com/v1.0",
    "Scopes": ["User.Read", "Mail.Read"]
  }
}
```

### Calling Microsoft Graph

```csharp
using Microsoft.Graph;
using Microsoft.Identity.Web;

[Authorize]
public class ProfileController : Controller
{
    private readonly GraphServiceClient _graphServiceClient;

    public ProfileController(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _graphServiceClient.Me.GetAsync();

        ViewData["DisplayName"] = user?.DisplayName;
        ViewData["Email"] = user?.Mail ?? user?.UserPrincipalName;

        return View();
    }
}
```

---

## Authorization Policies

### Role-Based Authorization

```csharp
// Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("roles", "Admin"));

    options.AddPolicy("RequireGroup", policy =>
        policy.RequireClaim("groups", "your-group-id"));
});

// Controller
[Authorize(Policy = "AdminOnly")]
public IActionResult AdminDashboard()
{
    return View();
}
```

---

## Migration Notes

- **Forms Authentication to Entra ID**: Replace `FormsAuthentication` with OIDC-based authentication
- **Windows Authentication**: Can coexist with Entra ID or migrate fully
- **Custom Membership Providers**: Migrate user data to Entra ID or Azure AD B2C
- **Session State**: User identity is maintained via secure cookies and tokens
- **Role Management**: Use Entra ID App Roles instead of ASP.NET Roles
