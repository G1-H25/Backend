// configuration/GetAuthenicationStrings.cs

namespace GpsApp.Configuration
{
    public static class AuthenticationConfigurationExtensions
    {
        public static string GetResolvedAudience(this IConfiguration config)
        {
            var audience =
                config["Authentication:ValidAudience"]
                ?? Environment.GetEnvironmentVariable("AUTHENICATION__VALIDAUDIENCE")
                ?? Environment.GetEnvironmentVariable("AUTH_VALIDAUDIENCE")
                ?? Environment.GetEnvironmentVariable("APPSETTING_AUTHENICATION__VALIDAUDIENCE");

            if (string.IsNullOrWhiteSpace(audience))
                throw new InvalidOperationException("ValidAudience is missing. Set it in configuration or environment variables.");

            return audience;
        }

        public static string GetResolvedIssuer(this IConfiguration config)
        {
            var issuer =
                config["Authentication:ValidIssuer"]
                ?? Environment.GetEnvironmentVariable("AUTHENICATION__VALIDISSUER")
                ?? Environment.GetEnvironmentVariable("AUTH_VALIDISSUER")
                ?? Environment.GetEnvironmentVariable("APPSETTING_AUTHENICATION__VALIDISSUER");

            if (string.IsNullOrWhiteSpace(issuer))
                throw new InvalidOperationException("ValidIssuer is missing. Set it in configuration or environment variables.");

            return issuer;
        }

        public static string GetResolvedSecretKey(this IConfiguration config)
        {
            var secret =
                config["Authentication:Secret_Key"]
                ?? Environment.GetEnvironmentVariable("AUTHENICATION__SECRET_KEY")
                ?? Environment.GetEnvironmentVariable("AUTH_SECRET_KEY")
                ?? Environment.GetEnvironmentVariable("APPSETTING_AUTHENICATION__SECRET_KEY");

            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("Secret_Key is missing. Set it in configuration or environment variables.");

            return secret;
        }
    }
}
