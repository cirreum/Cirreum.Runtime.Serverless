namespace Cirreum.Runtime;

using Azure.Core;
using Azure.Identity;
using Cirreum.RemoteServices;
using Cirreum.Runtime.RemoteServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public static class HostingExtensions {

	// Use a concurrent dictionary for thread safety
	private static readonly ConcurrentDictionary<string, (IHttpClientBuilder, ServerlessRemoteOptions)> NamedBuilders =
		new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Adds a remote service client for Azure Functions with proper authentication handling.
	/// </summary>
	/// <typeparam name="TClient">The type of the remote client.</typeparam>
	/// <param name="builder">The current <see cref="IServerlessDomainApplicationBuilder"/></param>
	/// <param name="configureOptions">The callback to configure the <see cref="ServerlessRemoteOptions"/>.</param>
	/// <param name="clientName">The optional name of the remote client. By default is set to the type name of the <typeparamref name="TClient"/>.</param>
	/// <returns>The specified <see cref="IServerlessDomainApplicationBuilder"/> instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown when options are invalid or client registration fails.</exception>
	public static IServerlessDomainApplicationBuilder AddRemoteClient<TClient>(
		this IServerlessDomainApplicationBuilder builder,
		Action<ServerlessRemoteOptions> configureOptions,
		string? clientName = null)
		where TClient : class {

		ArgumentNullException.ThrowIfNull(builder);

		// Normalize client name
		clientName ??= typeof(TClient).Name;

		// Create and configure options
		var options = new ServerlessRemoteOptions();
		configureOptions?.Invoke(options);

		return RegisterRemoteServiceClient<TClient>(builder, clientName, options);
	}

	/// <summary>
	/// Adds a remote service client for Azure Functions with proper authentication handling using a pre-configured options object.
	/// </summary>
	/// <typeparam name="TClient">The type of the remote client.</typeparam>
	/// <param name="builder">The current <see cref="IServerlessDomainApplicationBuilder"/>.</param>
	/// <param name="options">The already configured <see cref="ServerlessRemoteOptions"/>.</param>
	/// <param name="clientName">The optional name of the remote client. By default is set to the type name of the <typeparamref name="TClient"/>.</param>
	/// <returns>The specified <see cref="IServerlessDomainApplicationBuilder"/> instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when builder or options is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown when options are invalid or client registration fails.</exception>
	public static IServerlessDomainApplicationBuilder AddRemoteClient<TClient>(
		this IServerlessDomainApplicationBuilder builder,
		ServerlessRemoteOptions options,
		string? clientName = null)
		where TClient : class {

		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(options);

		// Normalize client name
		clientName ??= typeof(TClient).Name;

		return RegisterRemoteServiceClient<TClient>(builder, clientName, options);
	}

	/// <summary>
	/// Core implementation for registering a remote service client.
	/// </summary>
	private static IServerlessDomainApplicationBuilder RegisterRemoteServiceClient<TClient>(
		IServerlessDomainApplicationBuilder builder,
		string clientName,
		ServerlessRemoteOptions options)
		where TClient : class {

		// Validate options
		ValidateOptions(options);

		// Check for existing registration with same name but different options
		if (NamedBuilders.TryGetValue(clientName, out var existingRegistration)) {
			(_, var existingOptions) = existingRegistration;
			if (!existingOptions.ToJson().Equals(options.ToJson())) {
				throw new InvalidOperationException(
					$"A client named '{clientName}' is already registered with different options.");
			}

			// Return if already registered with same options
			return builder;
		}

		// Register appropriate credential based on credential type
		RegisterCredential(builder, options);

		// Configure the HTTP client
		var clientBuilder = ConfigureHttpClient<TClient>(builder, clientName, options);

		// Store the registration
		if (!NamedBuilders.TryAdd(clientName, (clientBuilder, options))) {
			// Handle race condition
			if (NamedBuilders.TryGetValue(clientName, out var existing)) {
				if (!existing.Item2.ToJson().Equals(options.ToJson())) {
					throw new InvalidOperationException(
						$"A client named '{clientName}' is already registered with different options.");
				}
			}
		}

		return builder;
	}

	/// <summary>
	/// Validates the ServerlessRemoteOptions.
	/// </summary>
	private static void ValidateOptions(ServerlessRemoteOptions options) {

		if (string.IsNullOrWhiteSpace(options.ServiceUri.ToString())) {
			throw new InvalidOperationException("ServiceUrl is required but was not provided.");
		}

		// Validate credential-specific options
		if (options.CredentialType == CredentialType.ClientSecret && options.SecretCredentialOptions == null) {
			throw new InvalidOperationException(
				"SecretCredentialOptions must be provided when CredentialType is set to ClientSecret.");
		}

		// Validate credential-specific options
		if ((options.CredentialType == CredentialType.ManagedIdentity || options.CredentialType == CredentialType.ClientSecret)
			&& string.IsNullOrWhiteSpace(options.AuthorityHost.ToString())) {
			throw new InvalidOperationException(
				"AuthorityHost must be provided when CredentialType is set to ManagedIdentity or ClientSecret.");
		}

		if (options.CredentialType == CredentialType.AuthorizationHeader && options.AuthorizationHeader == null) {
			throw new InvalidOperationException(
				"AuthorizationHeaderSettings must be provided when CredentialType is set to AuthorizationHeader.");
		}

	}

	/// <summary>
	/// Registers the appropriate credential based on the options.
	/// </summary>
	private static void RegisterCredential(IServerlessDomainApplicationBuilder builder, ServerlessRemoteOptions options) {

		// Only register if not already registered
		if (builder.Services.Any(sd => sd.ServiceType == typeof(TokenCredential))) {
			return;
		}

		switch (options.CredentialType) {
			case CredentialType.ClientSecret when options.SecretCredentialOptions != null:
				builder.Services.AddSingleton<TokenCredential>(sp => {
					return new ClientSecretCredential(
						options.SecretCredentialOptions.TenantId,
						options.SecretCredentialOptions.ClientId,
						options.SecretCredentialOptions.ClientSecret,
						new ClientSecretCredentialOptions {
							AuthorityHost = options.AuthorityHost
						});
				});
				break;

			case CredentialType.ManagedIdentity:
				builder.Services.AddSingleton<TokenCredential>(sp => {
					return new DefaultAzureCredential(new DefaultAzureCredentialOptions {
						AuthorityHost = options.AuthorityHost,
						// In Function Apps, exclude these credentials
						ExcludeVisualStudioCredential = true,
						ExcludeAzureCliCredential = true,
						ExcludeAzureDeveloperCliCredential = true,
						ExcludeAzurePowerShellCredential = true
					});
				});
				break;
		}
	}

	/// <summary>
	/// Configures the HTTP client with the appropriate settings.
	/// </summary>
	private static IHttpClientBuilder ConfigureHttpClient<TClient>(
		IServerlessDomainApplicationBuilder builder,
		string clientName,
		ServerlessRemoteOptions options)
		where TClient : class {

		var clientBuilder = builder.Services.AddHttpClient<TClient>((client) => {
			client.BaseAddress = options.ServiceUri;
			if (!string.IsNullOrWhiteSpace(options.ApplicationName)) {
				client.DefaultRequestHeaders.Add(RemoteIdentityConstants.AppNameHeader, options.ApplicationName);
			}
		});

		// Add authentication
		ConfigureAuthentication(clientBuilder, clientName, options);

		// Add header redaction if needed
		if (options.RedactedHeaders.Count != 0) {
			clientBuilder.RedactLoggedHeaders(options.RedactedHeaders);
		}

		return clientBuilder;
	}

	/// <summary>
	/// Configures authentication for the HTTP client.
	/// </summary>
	private static void ConfigureAuthentication(IHttpClientBuilder clientBuilder, string clientName, ServerlessRemoteOptions options) {
		switch (options.CredentialType) {
			case CredentialType.ClientSecret:
			case CredentialType.ManagedIdentity:
				clientBuilder.AddHttpMessageHandler(sp => {
					var credential = sp.GetRequiredService<TokenCredential>();
					var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger($"BearerAuth:{clientName}");
					return new BearerAuthenticationHandler(credential, [.. options.ServiceScopes], logger);
				});
				break;

			case CredentialType.AuthorizationHeader when options.AuthorizationHeader != null:
				clientBuilder.ConfigureHttpClient(client => {
					client.DefaultRequestHeaders.Authorization =
						new System.Net.Http.Headers.AuthenticationHeaderValue(
							options.AuthorizationHeader.Scheme,
							options.AuthorizationHeader.Value);
				});
				break;
		}
	}

	/// <summary>
	/// Handler for adding bearer tokens from credential providers.
	/// </summary>
	private class BearerAuthenticationHandler(
		TokenCredential credential,
		string[] scopes,
		ILogger logger,
		TimeSpan? tokenRefreshBuffer = null) : DelegatingHandler {

		private readonly TokenCredential _credential = credential ?? throw new ArgumentNullException(nameof(credential));
		private readonly string[] _scopes = scopes?.Length > 0 ? scopes : throw new ArgumentException("At least one scope must be provided", nameof(scopes));
		private readonly TimeSpan _tokenRefreshBuffer = tokenRefreshBuffer ?? TimeSpan.FromSeconds(45);

		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken) {
			try {

				// Get token using the static cache
				var token = await TokenCache.GetTokenAsync(
					_credential,
					_scopes,
					_tokenRefreshBuffer,
					cancellationToken);

				request.Headers.Authorization =
					new System.Net.Http.Headers.AuthenticationHeaderValue(
						"Bearer",
						token.Token);

				return await base.SendAsync(request, cancellationToken);

			} catch (Exception ex) {
				logger?.LogError(ex, "Error getting authentication token for remote service call");
				throw;
			}
		}
	}

}