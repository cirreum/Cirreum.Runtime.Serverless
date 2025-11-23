# Cirreum.Runtime.Serverless

[![NuGet Version](https://img.shields.io/nuget/v/Cirreum.Runtime.Serverless.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.Runtime.Serverless/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Cirreum.Runtime.Serverless.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.Runtime.Serverless/)
[![GitHub Release](https://img.shields.io/github/v/release/cirreum/Cirreum.Runtime.Serverless?style=flat-square&labelColor=1F1F1F&color=FF3B2E)](https://github.com/cirreum/Cirreum.Runtime.Serverless/releases)
[![License](https://img.shields.io/github/license/cirreum/Cirreum.Runtime.Serverless?style=flat-square&labelColor=1F1F1F&color=F2F2F2)](https://github.com/cirreum/Cirreum.Runtime.Serverless/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-003D8F?style=flat-square&labelColor=1F1F1F)](https://dotnet.microsoft.com/)

**Domain-driven runtime abstractions for Azure Functions serverless applications**

## Overview

**Cirreum.Runtime.Serverless** provides a comprehensive runtime abstraction layer for building domain-driven Azure Functions applications. It extends the Azure Functions Worker SDK with opinionated patterns for dependency injection, remote service communication, and application startup while maintaining the flexibility needed for serverless architectures.

## Key Features

- **Domain Application Builder** - Fluent API extending `FunctionsApplicationBuilder` for configuring domain-driven serverless applications
- **Remote Service Integration** - Built-in HTTP client configuration with multiple authentication methods (Managed Identity, Client Secret, Authorization Header)
- **Authorization Framework** - Role-based authorization with automatic service initialization
- **Startup Tasks** - Extensible initialization system for application startup
- **Token Management** - Automatic bearer token handling with intelligent caching
- **Cloud-Native Security** - Deep integration with Azure Identity for secure authentication

## Quick Start

```csharp
using Cirreum.Runtime.Serverless;

var builder = DomainApplication.CreateBuilder(args);

// Configure services
builder.Services.AddDomainServices();
builder.Services.AddRemoteService("MyApi", options =>
{
    options.BaseAddress = "https://api.example.com";
    options.AuthenticationMode = RemoteServiceAuthenticationMode.ManagedIdentity;
});

// Build and run
var app = builder.Build();
await app.RunAsync();
```

## Remote Service Configuration

The library provides comprehensive support for inter-service communication in serverless environments:

```csharp
builder.Services.AddRemoteService("ServiceName", options =>
{
    options.BaseAddress = "https://service.example.com";
    options.AuthenticationMode = RemoteServiceAuthenticationMode.ClientSecret;
    options.ClientId = "your-client-id";
    options.ClientSecret = "your-client-secret"; // Use Azure Key Vault in production
    options.Scopes = new[] { "api://service/.default" };
});
```

### Authentication Modes

- **ManagedIdentity** - Uses Azure Managed Identity for authentication
- **ClientSecret** - Traditional client credentials flow
- **AuthorizationHeader** - Custom authorization header support
- **None** - No authentication (for public APIs)

## Architecture

The library follows domain-driven design principles with clear separation of concerns:

```
DomainApplication (Entry Point)
    ├── DomainApplicationBuilder (Configuration)
    ├── Remote Services (External Communication)
    ├── Authorization (Security)
    └── Startup Tasks (Initialization)
```

## Contribution Guidelines

1. **Be conservative with new abstractions**  
   The API surface must remain stable and meaningful.

2. **Limit dependency expansion**  
   Only add foundational, version-stable dependencies.

3. **Favor additive, non-breaking changes**  
   Breaking changes ripple through the entire ecosystem.

4. **Include thorough unit tests**  
   All primitives and patterns should be independently testable.

5. **Document architectural decisions**  
   Context and reasoning should be clear for future maintainers.

6. **Follow .NET conventions**  
   Use established patterns from Microsoft.Extensions.* libraries.

## Versioning

Cirreum.Runtime.Serverless follows [Semantic Versioning](https://semver.org/):

- **Major** - Breaking API changes
- **Minor** - New features, backward compatible
- **Patch** - Bug fixes, backward compatible

Given its foundational role, major version bumps are rare and carefully considered.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Cirreum Foundation Framework**  
*Layered simplicity for modern .NET*