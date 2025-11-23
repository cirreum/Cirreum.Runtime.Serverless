# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Restore dependencies
dotnet restore Cirreum.Runtime.Serverless.slnx

# Build the solution (Debug)
dotnet build Cirreum.Runtime.Serverless.slnx

# Build the solution (Release)
dotnet build Cirreum.Runtime.Serverless.slnx --configuration Release

# Create NuGet packages
dotnet pack Cirreum.Runtime.Serverless.slnx --configuration Release --output ./artifacts

# Clean build outputs
dotnet clean Cirreum.Runtime.Serverless.slnx
```

## Code Architecture

### Overview
This is a .NET library that provides a runtime abstraction layer for Azure Functions serverless applications using the Cirreum framework. It implements domain-driven design (DDD) patterns with CQRS features.

### Key Components

1. **DomainApplicationBuilder** (`src/Cirreum.Runtime.Serverless/DomainApplicationBuilder.cs`)
   - Extends `FunctionsApplicationBuilder` for domain-driven serverless applications
   - Entry point: `DomainApplication.CreateBuilder(args)`
   - Handles dependency injection, configuration, and service registration

2. **Remote Services** (`src/Cirreum.Runtime.Serverless/RemoteServices/`)
   - HTTP client configuration for inter-service communication
   - Multiple authentication methods: ManagedIdentity, ClientSecret, AuthorizationHeader
   - Automatic bearer token handling with caching

3. **Authorization** (`src/Cirreum.Runtime.Serverless/Authorization/`)
   - Role registry integration with Cirreum authorization framework
   - Auto-initialization of authorization services

4. **Startup Tasks** (`src/Cirreum.Runtime.Serverless/StartupTasks/`)
   - Plugin-based initialization system
   - Executes tasks during application startup

### Project Structure
```
src/Cirreum.Runtime.Serverless/
├── Authorization/           # Authorization components
├── Extensions/Hosting/      # Extension methods for hosting
├── RemoteServices/         # Remote service client configuration
└── StartupTasks/           # Application startup tasks
```

### Important Dependencies
- **Framework**: .NET 10.0 (cutting-edge/pre-release)
- **Azure Functions**: Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore
- **Cirreum**: Cirreum.Startup, Cirreum.Logging.Deferred, Cirreum.Services.Serverless
- **Authentication**: Azure.Identity

### Usage Pattern
Applications using this library typically:
1. Call `DomainApplication.CreateBuilder(args)` to create an application builder
2. Configure services using the builder's extension methods
3. Build and run the Azure Functions host

### Development Notes
- The solution uses modern .slnx format instead of traditional .sln
- Build configuration is managed through Directory.Build.props and files in `/build/`
- XML documentation generation is enabled
- Nullable reference types are enabled
- Latest C# language features are used