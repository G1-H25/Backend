// Composition/ServiceRegistration.cs

using GpsApp.Controllers;
using GpsApp.Services;

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
                services.AddSingleton<ISqlGet>(_ => new SqlGet(connectionString));
                services.AddSingleton<ISqlGetAdvanced>(_ => new SqlGetAdvanced(connectionString));
                services.AddSingleton(_ => new SqlUpdate(connectionString));
                services.AddScoped<GetUser>();
                services.AddScoped<HealthController>();
                services.AddScoped<IAuthorizationService, AuthorizationService>();
                services.AddScoped<ISensorValidationService, SensorValidationService>();
            }

            return services;
        }
    }
}


