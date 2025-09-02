using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GpsSensorApi
{
    public class Startup
    {
        // Provides access to app configuration (e.g., appsettings.json, environment variables).
        public IConfiguration Configuration { get; }

        // Constructor that receives the configuration object via dependency injection.
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // Registers services needed by the app (e.g., controllers, DB, CORS, Swagger).
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(); // Enables support for controller-based APIs.

            // Register other services here (e.g., EF Core, CORS, Swagger, auth, etc.).
        }

        // Sets up the middleware pipeline that handles incoming HTTP requests.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // Shows detailed error pages in development.
            }

            app.UseRouting(); // Adds routing capabilities to match requests to endpoints.

            app.UseAuthorization(); // Enables authorization checks for protected endpoints.

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // Maps controller actions to endpoints.
            });

            // Add optional middleware like HTTPS redirection, CORS, Swagger, authentication here.
        }
    }
}

