namespace Cirreum.Runtime.RemoteServices;

using Cirreum.RemoteServices;

/// <summary>
/// Defines the type of credential to use for authentication.
/// </summary>
public enum CredentialType {
	/// <summary>
	/// Use managed identity credential for authentication (typically for production environments).
	/// </summary>
	ManagedIdentity,
	/// <summary>
	/// User supplied authorization header settings.
	/// </summary>
	/// <remarks>
	/// See: <see cref="AuthorizationHeaderSettings"/>
	/// </remarks>
	AuthorizationHeader,
	/// <summary>
	/// Use client secret credential for authentication (typically for development environments).
	/// </summary>
	ClientSecret
}