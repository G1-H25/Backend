// configuration/GetAuthenticationStrings.cs

namespace GpsApp.Configuration
{
    public static class AuthenticationConfigurationExtensions
    {
        public static string GetResolvedAudience(this IConfiguration config)
        {
            var audience =
                config["Authentication:ValidAudience"]
                ?? Environment.GetEnvironmentVariable("AUTHENTICATION__VALIDAUDIENCE")
                ?? Environment.GetEnvironmentVariable("AUTH_VALIDAUDIENCE")
                ?? Environment.GetEnvironmentVariable("APPSETTING_AUTHENTICATION__VALIDAUDIENCE");

            Console.WriteLine($"DEBUG: Audience from config/env: '{audience ?? "null"}'");

//            if (string.IsNullOrWhiteSpace(audience))
//                throw new InvalidOperationException("ValidAudience is missing. Set it in configuration or environment variables.");

            return audience;
        }

        public static string GetResolvedIssuer(this IConfiguration config)
        {
            var issuer =
                config["Authentication:ValidIssuer"]
                ?? Environment.GetEnvironmentVariable("AUTHENTICATION__VALIDISSUER")
                ?? Environment.GetEnvironmentVariable("AUTH_VALIDISSUER")
                ?? Environment.GetEnvironmentVariable("APPSETTING_AUTHENTICATION__VALIDISSUER");

            Console.WriteLine($"DEBUG: Issuer from config/env: '{issuer ?? "null"}'");

//              if (string.IsNullOrWhiteSpace(issuer))
//                  throw new InvalidOperationException("ValidIssuer is missing. Set it in configuration or environment variables.");

            return issuer;
        }

        public static string GetResolvedSecretKey(this IConfiguration config)
        {
            var secret =
                config["Authentication:Secret_Key"]
                ?? Environment.GetEnvironmentVariable("AUTHENTICATION__SECRET_KEY")
                ?? Environment.GetEnvironmentVariable("AUTH_SECRET_KEY")
                ?? Environment.GetEnvironmentVariable("APPSETTING_AUTHENTICATION__SECRET_KEY");

            Console.WriteLine($"DEBUG: SecretKey from config/env: '{(string.IsNullOrWhiteSpace(secret) ? "null or empty" : "set")}'");

//              if (string.IsNullOrWhiteSpace(secret))
//                  throw new InvalidOperationException("Secret_Key is missing. Set it in configuration or environment variables.");

            return secret;
        }
    }
}
