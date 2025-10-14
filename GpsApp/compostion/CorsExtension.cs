namespace GpsApp.Composition
{
    public static class CorsExtension
    {
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("https://zealous-desert-0bb104903.1.azurestaticapps.net")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}