// Extensions/ApplicationBuilderExtensions.cs

namespace GpsApp.Middleware
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseApplicationMiddleware(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // if we want to stage something
            if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
            {
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "GPS App V2 API");
                c.RoutePrefix = "swagger"; // UI at /swagger/index.html
                c.DocumentTitle = "GPS App V2 API Documentation";
                c.DefaultModelsExpandDepth(-1); // Hide models section by default
                c.DisplayRequestDuration();
                c.EnableDeepLinking();
                c.EnableFilter();
                c.ShowExtensions();
                c.EnableValidator();
            });

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseCors("AllowFrontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
        }
    }
}