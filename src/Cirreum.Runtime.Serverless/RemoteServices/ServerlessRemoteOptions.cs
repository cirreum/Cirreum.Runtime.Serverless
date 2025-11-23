namespace Cirreum.Runtime.RemoteServices;

using Azure.Identity;
using Cirreum.RemoteServices;

/// <summary>
/// Represents configuration options for serverless remote services.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="RemoteServiceOptions.ApplicationName"/> is added to the request headers and
/// consumed on the remote server as the UserName of the Principal Identity.
/// </para>
/// This class extends <see cref="RemoteServiceOptions"/> to provide additional configuration specific to
/// serverless environments, particularly around authentication methods.
/// </remarks>
/// <inheritdoc cref="RemoteServiceOptions"/>
public class ServerlessRemoteOptions : RemoteServiceOptions {

	/// <summary>
	/// Default constructor
	/// </summary>
	public ServerlessRemoteOptions()
		: base() {

	}

	/// <summary>
	/// Construct a new instance with the specified application name.
	/// </summary>
	/// <param name="applicationName">The name of the client application making the call to the remote service.</param>
	public ServerlessRemoteOptions(string applicationName)
		: base(applicationName) {

	}

	/// <summary>
	/// Construct a new instance with the specified application name and service url.
	/// </summary>
	/// <param name="applicationName">The name of the client application making the call to the remote service.</param>
	/// <param name="serviceUri">The base Uri for the remote service</param>
	public ServerlessRemoteOptions(string applicationName, Uri serviceUri)
		: base(applicationName, serviceUri) {
	}

	/// <summary>
	/// The host of the Microsoft Entra authority.
	/// </summary>
	/// <value>
	/// Defaults to <see cref="AzureAuthorityHosts.AzurePublicCloud"/> (https://login.microsoftonline.com/).
	/// </value>
	/// <exception cref="ArgumentNullException">Thrown when set to null.</exception>
	public Uri AuthorityHost {
		get => _authorityHost;
		set => _authorityHost = value ?? throw new ArgumentNullException(nameof(value));
	}
	private Uri _authorityHost = AzureAuthorityHosts.AzurePublicCloud;

	/// <summary>
	/// Gets or sets the type of credentials to use for authentication.
	/// </summary>
	/// <value>
	/// Defaults to <see cref="CredentialType.ManagedIdentity"/> for serverless environments.
	/// </value>
	public CredentialType CredentialType { get; set; } = CredentialType.ManagedIdentity;

	/// <summary>
	/// Gets or sets the optional Secret Credential Options when <see cref="CredentialType"/>
	/// equals <see cref="CredentialType.ClientSecret"/>.
	/// </summary>
	/// <remarks>
	/// This is typically only used during development.
	/// </remarks>
	public SecretCredentialOptions? SecretCredentialOptions { get; set; }

}