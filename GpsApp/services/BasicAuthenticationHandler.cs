using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace GpsApp.Services;

/// <summary>
/// Basic Authentication handler for gateway devices
/// Validates credentials against gateway table in database
/// </summary>
public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ISqlGet _getService;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        ISqlGet getService)
        : base(options, logger, encoder, clock)
    {
        _getService = getService ?? throw new ArgumentNullException(nameof(getService));
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("Missing Authorization header");
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Invalid Authorization header format");
        }

        try
        {
            var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
            var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var credentials = decodedCredentials.Split(':', 2);

            if (credentials.Length != 2)
            {
                return AuthenticateResult.Fail("Invalid credentials format");
            }

            var gatewayId = credentials[0];
            var password = credentials[1];

            // Validate gateway credentials against database
            var gateway = await _getService.FetchAsync("Secrets.Gateway", new Dictionary<string, object>
            {
                { "Id", gatewayId }
            });

            if (gateway == null)
            {
                return AuthenticateResult.Fail("Invalid gateway credentials");
            }

            // For now, we'll use a simple password validation
            // In production, you'd want to hash passwords and compare hashes
            var expectedPassword = gateway["Password"]?.ToString();
            if (string.IsNullOrEmpty(expectedPassword) || expectedPassword != password)
            {
                return AuthenticateResult.Fail("Invalid gateway credentials");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, gatewayId),
                new Claim(ClaimTypes.Name, $"Gateway_{gatewayId}"),
                new Claim("GatewayId", gatewayId),
                new Claim("UserId", gateway["UserId"]?.ToString() ?? "0")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during basic authentication");
            return AuthenticateResult.Fail("Authentication error");
        }
    }
}
