// Composition/ServiceRegistration.cs

using GpsApp.Controllers;

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
                services.AddScoped<GetUser>();
                services.AddScoped<HealthController>();
                services.AddScoped<DeliveryGetController>();
            }

            return services;
        }
    }
}


