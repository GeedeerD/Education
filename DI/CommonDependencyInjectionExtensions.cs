using DataBase;
using DataBase.Interfaces;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CommonDependencyInjectionExtensions
    {
        public static IServiceCollection AddCommonDependency(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddScoped<IMessageRepository, MessageRepository>()
                .AddScoped<IMongoDbContext, MongoDbContext>();

            return services;
        }
    }
}
