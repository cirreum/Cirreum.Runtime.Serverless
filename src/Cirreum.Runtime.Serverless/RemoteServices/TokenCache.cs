namespace Cirreum.Runtime.RemoteServices;

using Azure.Core;
using System.Collections.Concurrent;

/// <summary>
/// A static cache that can be shared across function invocations in the same instance
/// </summary>
internal static class TokenCache {

	private static readonly ConcurrentDictionary<string, AccessToken> _tokenCache = new();
	private static readonly ConcurrentDictionary<string, SemaphoreSlim> _lockObjects = new();

	// Create a unique key for each scope set
	private static string CreateCacheKey(string[] scopes) => string.Join('|', scopes);

	public static async Task<AccessToken> GetTokenAsync(
		TokenCredential credential,
		string[] scopes,
		TimeSpan? refreshBuffer = null,
		CancellationToken cancellationToken = default) {

		var cacheKey = CreateCacheKey(scopes);
		var bufferTime = refreshBuffer ?? TimeSpan.FromSeconds(45);

		// Try to get token from cache
		if (_tokenCache.TryGetValue(cacheKey, out var cachedToken) &&
			DateTimeOffset.Now.Add(bufferTime) < cachedToken.ExpiresOn) {
			return cachedToken;
		}

		// Get or create a lock object for this cache key
		var lockObj = _lockObjects.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));

		try {
			await lockObj.WaitAsync(cancellationToken);

			// Double-check after acquiring lock
			if (_tokenCache.TryGetValue(cacheKey, out cachedToken) &&
				DateTimeOffset.Now.Add(bufferTime) < cachedToken.ExpiresOn) {
				return cachedToken;
			}

			// Get a new token
			var newToken = await credential.GetTokenAsync(
				new TokenRequestContext(scopes),
				cancellationToken);

			// Update cache
			_tokenCache[cacheKey] = newToken;

			return newToken;

		} finally {
			lockObj.Release();
		}
	}

	// Optional: Add a method to clear the cache if needed
	public static void ClearCache() => _tokenCache.Clear();

}