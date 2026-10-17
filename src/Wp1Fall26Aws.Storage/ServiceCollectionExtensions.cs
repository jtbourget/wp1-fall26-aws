using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wp1Fall26Aws.Storage;

/// <summary>
/// Dependency injection extension methods for registering document storage services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers document storage options, Amazon S3 client boundary, document store,
    /// and document repository implementations into the service collection.
    /// </summary>
    /// <param name="services">The target <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">The application configuration root containing the "S3Storage" configuration section.</param>
    /// <returns>The modified <see cref="IServiceCollection"/> for fluent chaining.</returns>
    public static IServiceCollection AddStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<S3StorageOptions>(configuration.GetSection("S3Storage"));
        services.AddSingleton<IS3ObjectClient, AwsS3ObjectClient>();
        services.AddSingleton<IDocumentStore, S3DocumentStore>();
        services.AddSingleton<IDocumentRepository, InMemoryDocumentRepository>();
        
        return services;
    }
}
