namespace Cirreum.Runtime;

/// <summary>
/// Provides factory methods for creating serverless domain application builders.
/// </summary>
public static class DomainApplication {
	/// <summary>
	/// Creates and configures a new <see cref="DomainApplicationBuilder"/> instance for Azure Function applications.
	/// </summary>
	/// <param name="args">Command line arguments passed to the application.</param>
	/// <returns>
	/// A configured <see cref="DomainApplicationBuilder"/> instance ready for further customization.
	/// </returns>
	/// <remarks>
	/// This factory method creates a builder preconfigured for serverless function applications.
	/// It sets up the Functions runtime, configures core services, and registers system initializers, 
	/// auto-initialization services, and startup tasks.
	/// </remarks>
	/// <example>
	/// <code>
	/// var builder = DomainApplication.CreateBuilder(args);
	/// 
	/// // Add application-specific services
	/// builder.Services.AddSingleton&lt;IOrderProcessor, OrderProcessor&gt;();
	/// 
	/// // Cloud services
	/// builder.AddPersistence();
	/// builder.AddMessaging();
	/// 
	/// // Build and run the application
	/// await builder.BuildAndRunAsync&lt;MyDomainType&gt;();
	/// </code>
	/// </example>
	public static DomainApplicationBuilder CreateBuilder(string[] args) {
		return DomainApplicationBuilder.CreateAndConfigureBuilder(args);
	}
}