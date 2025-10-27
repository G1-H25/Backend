// Composition/ServiceRegistration.cs

using GpsApp.Controllers;
using GpsApp.Infrastructure.Data;
using GpsApp.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

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
                // Add Entity Framework Core
                services.AddDbContext<GpsAppDbContext>(options =>
                    options.UseSqlServer(connectionString));

                // Add repositories
                services.AddScoped<IShipmentRepository, ShipmentRepository>();
                services.AddScoped<IPackageRepository, PackageRepository>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();

                // Legacy SQL services (can be removed once fully migrated to EF Core)
                services.AddSingleton(_ => new SqlInsert(connectionString));
                services.AddSingleton<ISqlGet>(_ => new SqlGet(connectionString));
                services.AddSingleton<ISqlGetAdvanced>(_ => new SqlGetAdvanced(connectionString));
                services.AddSingleton(_ => new SqlUpdate(connectionString));
                services.AddScoped<GetUser>();
                services.AddScoped<HealthController>();
                services.AddScoped<IAuthorizationService, AuthorizationService>();
            }

            return services;
        }
    }
}


