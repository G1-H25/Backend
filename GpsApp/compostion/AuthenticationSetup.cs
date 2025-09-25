// Composition/AuthenticationSetup.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GpsApp.Configuration;
using System.Security.Claims;

public static class AuthenticationSetup
{
    public static IServiceCollection AddAuthenticationAndAuthorization(
    this IServiceCollection services,
    IConfiguration config)  // Inject configuration here
    {

        var audience = config.GetResolvedAudience();
        var issuer = config.GetResolvedIssuer();
        var secret = config.GetResolvedSecretKey();

        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException("Secret_Key is not configured. Check appsettings or environment variables.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    ValidateAudience = true,
                    ValidAudience = audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),

                    RoleClaimType = ClaimTypes.Role,
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
            options.AddPolicy("NonUserOnly", policy => policy.RequireRole("NonUser"));
        });

        return services;
    }
}
