using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Configurations.Extensions;
public static class ConfigurationExtensions
{
    public static IServiceCollection AddAppConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(
            configuration.GetSection(MongoDbSettings.SectionName));

        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        return services;
    }
}