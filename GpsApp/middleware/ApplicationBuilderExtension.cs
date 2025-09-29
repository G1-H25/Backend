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
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
        }
    }
}