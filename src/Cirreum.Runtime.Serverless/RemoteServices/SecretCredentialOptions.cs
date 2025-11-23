namespace Cirreum.Runtime.RemoteServices;

/// <summary>
/// The options required  when <see cref="ServerlessRemoteOptions.CredentialType"/>
/// is set to <see cref="CredentialType.ClientSecret"/>.
/// </summary>
/// <param name="TenantId"></param>
/// <param name="ClientId"></param>
/// <param name="ClientSecret"></param>
public record SecretCredentialOptions(
	string TenantId,
	string ClientId,
	string ClientSecret
);