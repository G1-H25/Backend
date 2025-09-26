// Composition/ServiceRegistration.cs

namespace GpsApp.Composition
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string? connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddSingleton(_ => new SqlInsert(connectionString));
                services.AddSingleton(_ => new SqlGet(connectionString));
                services.AddSingleton(_ => new GetUser(connectionString));
            }

            return services;
        }
    }
}


