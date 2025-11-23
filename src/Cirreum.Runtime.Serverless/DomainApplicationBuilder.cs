namespace Cirreum.Runtime;

using Cirreum.Conductor.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

/// <summary>
/// A builder for Azure Functions applications that integrates domain services,
/// authorization, validation, and CQRS features.
/// </summary>
/// <remarks>
/// <para>
/// This builder extends the standard FunctionsApplicationBuilder with additional features
/// for domain-driven serverless applications. It provides configuration for Azure Functions,
/// system initialization, and common infrastructure services.
/// </para>
/// <para>
/// Use the <see cref="DomainApplication.CreateBuilder"/> method to create a pre-configured
/// instance of this builder. Then, configure additional services as needed before
/// calling one of the <see cref="BuildAndRunAsync()"/> methods.
/// </para>
/// <para>
/// The builder supports specifying additional assemblies containing domain services,
/// validators, and authorization handlers through the <see cref="DomainServicesBuilder"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var builder = DomainApplication.CreateBuilder(args);
/// 
/// // Add application services
/// builder.Services.AddScoped&lt;IOrderService, OrderService&gt;();
/// 
/// // Configure Azure Services
/// builder.AddPersistence();
/// builder.AddMessaging();
/// 
/// // Build and run the application with domain services
/// await builder.BuildAndRunAsync&lt;MyDomainType&gt;();
/// </code>
/// </example>
public sealed class DomainApplicationBuilder
	: IServerlessDomainApplicationBuilder, IHostApplicationBuilder {

	private Action<ConductorOptionsBuilder>? _conductorConfiguration;

	internal static DomainApplicationBuilder CreateAndConfigureBuilder(string[] args) {

		var functionBuilder = new DomainApplicationBuilder {
			FunctionsApplicationBuilder = FunctionsApplication.CreateBuilder(args)
		};

		functionBuilder.FunctionsApplicationBuilder.ConfigureFunctionsWebApplication();

		// Domain Environment
		functionBuilder.Services.AddSingleton<IDomainEnvironment, DomainEnvironment>();

		functionBuilder.AddCoreServices();

		functionBuilder.Services.AddApplicationInitializers();

		return functionBuilder;

	}

	/// <inheritdoc/>
	public required FunctionsApplicationBuilder FunctionsApplicationBuilder { get; init; }

	/// <inheritdoc/>
	IConfigurationManager IHostApplicationBuilder.Configuration => this.FunctionsApplicationBuilder.Configuration;

	/// <inheritdoc/>
	IConfigurationManager IServerlessDomainApplicationBuilder.Configuration => this.FunctionsApplicationBuilder.Configuration;

	/// <summary>
	/// A collection of configuration providers for the application to compose. This is useful for adding new configuration sources and providers.
	/// </summary>
	public ConfigurationManager Configuration => this.FunctionsApplicationBuilder.Configuration;

	/// <inheritdoc/>
	public IHostEnvironment Environment => this.FunctionsApplicationBuilder.Environment;

	/// <inheritdoc/>
	public ILoggingBuilder Logging => this.FunctionsApplicationBuilder.Logging;

	/// <inheritdoc/>
	public IServiceCollection Services => this.FunctionsApplicationBuilder.Services;

	/// <inheritdoc/>
	IMetricsBuilder IHostApplicationBuilder.Metrics => this.FunctionsApplicationBuilder.Metrics;

	/// <inheritdoc/>
	IDictionary<object, object> IHostApplicationBuilder.Properties => this.FunctionsApplicationBuilder.Properties;


	/// <summary>
	/// Constructor
	/// </summary>
	private DomainApplicationBuilder() {

	}

	/// <inheritdoc/>
	void IHostApplicationBuilder.ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure) {
		((IHostApplicationBuilder)this.FunctionsApplicationBuilder).ConfigureContainer(factory, configure);
	}

	/// <inheritdoc/>
	public IDomainApplicationBuilder ConfigureConductor(Action<ConductorOptionsBuilder> configure) {
		ArgumentNullException.ThrowIfNull(configure);

		var previousConfig = _conductorConfiguration;
		_conductorConfiguration = options => {
			previousConfig?.Invoke(options);
			configure(options);
		};

		return this;
	}

	/// <summary>
	/// Registers and configures the domain services including authorization evaluators,
	/// documenters and CQRS features, and then builds the application and executes any
	/// registered <see cref="ISystemInitializer"/>, <see cref="IAutoInitialize"/> or
	/// <see cref="IStartupTask"/> services, and then runs the application, returning an awaitable
	/// Task that only completes when shutdown is triggered.
	/// </summary>
	/// <returns>
	/// A <see cref="Task"/> that represents the entire runtime of the <see cref="IHost"/> from startup to shutdown.
	/// </returns>
	/// <remarks>
	/// This method registers core domain services without specifying additional assemblies
	/// that should be scanned. If your domain services are in separate assemblies, consider using 
	/// the overloads that allow you to specify assemblies to scan.
	/// </remarks>
	public async Task BuildAndRunAsync() {

		var app = await this.BuildDomainCore();

		// ******************************************************************************
		// Run the application
		//
		await app.RunAsync();

	}

	/// <summary>
	/// Builds the application and executes any registered <see cref="ISystemInitializer"/>,
	/// <see cref="IAutoInitialize"/> or <see cref="IStartupTask"/> services, and then runs
	/// the application, returning an awaitable Task that only completes when shutdown is triggered.
	/// </summary>
	/// <param name="configureDomainServices">A callback to configure domain service assemblies.</param>
	/// <returns>
	/// A <see cref="Task"/> that represents the entire runtime of the <see cref="IHost"/> from startup to shutdown.
	/// </returns>
	/// <remarks>
	/// This method allows you to specify additional assemblies that should be scanned for domain services,
	/// validators, and authorization handlers. Use the provided <see cref="DomainServicesBuilder"/>
	/// to register assemblies containing your domain components.
	/// </remarks>
	/// <example>
	/// <code>
	/// builder.BuildAndRunAsync(domain => {
	///     domain.AddAssemblyContaining&lt;Asm1.GetOrders&gt;()
	///           .AddAssemblyContaining&lt;Asm2.GetUsers&gt;();
	/// });
	/// </code>
	/// </example>
	public Task BuildAndRunAsync(Action<DomainServicesBuilder> configureDomainServices) {

		// Build domain services if any...
		var domainBuilder = new DomainServicesBuilder();
		configureDomainServices(domainBuilder);

		return this.BuildAndRunAsync();
	}

	/// <summary>
	/// Builds the application and executes any registered <see cref="ISystemInitializer"/>,
	/// <see cref="IAutoInitialize"/> or <see cref="IStartupTask"/> services, and then runs
	/// the application, returning an awaitable Task that only completes when shutdown is triggered.
	/// </summary>
	/// <typeparam name="TDomainMarker">A type from the assembly containing domain services to register.</typeparam>
	/// <returns>
	/// A <see cref="Task"/> that represents the entire runtime of the <see cref="IHost"/> from startup to shutdown.
	/// </returns>
	/// <remarks>
	/// This is a convenience method that allows you to include an additional assembly containing the specified type.
	/// Use this method when your domain services are in a single separate assembly from your API.
	/// </remarks>
	/// <example>
	/// <code>
	/// builder.BuildAndRunAsync&lt;SomeDomainType&gt;();
	/// </code>
	/// </example>
	public Task BuildAndRunAsync<TDomainMarker>() {
		return this.BuildAndRunAsync(domain => domain.AddAssemblyContaining<TDomainMarker>());
	}

	private async Task<IHost> BuildDomainCore() {

		// ******************************************************************************
		// Final assembly
		//

		// All remaining common services
		this.Services.AddDefaultAuthorizationEvaluator();
		this.Services.AddDefaultAuthorizationDocumenter();

		// ******************************************************************************
		// App Domain - Conductor/FluentValidation/FluentAuthorization
		// If ConfigureConductor wasn't called, attempt to auto-bind from appsettings
		var conductorConfig = _conductorConfiguration ?? (options => options.BindConfiguration(this.Configuration));
		this.Services.AddDomainServices(conductorConfig);


		// Build the app!
		using var app = this.FunctionsApplicationBuilder.Build();

		// ******************************************************************************
		// Initialize the application
		//
		await app.Services.InitializeApplicationAsync();

		return app;

	}


}