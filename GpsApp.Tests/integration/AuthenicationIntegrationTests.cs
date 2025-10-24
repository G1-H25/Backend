using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

public class IntegrationTests : IClassFixture<TestFixture>
{
    private readonly HttpClient _client;

    public IntegrationTests(TestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task FullIntegrationFlow_ShouldSucceed()
    {
        // 1. Register company
        var companyName = $"TestCompany_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        var companyRequest = new
        {
            companyName = companyName,
            email = "test@example.com",
            address = new
            {
                street = "123 Test St",
                streetNumber = 10,
                postalCode = "12345",
                city = "Testville",
                country = "Testland"
            }
        };

        var companyResponse = await PostJsonAsync("/company/register", companyRequest);
        companyResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "Company registration should succeed");

        using var companyDoc = JsonDocument.Parse(await companyResponse.Content.ReadAsStringAsync());
        var companyId = companyDoc.RootElement.GetProperty("companyId").GetInt32();

        // 2. Signup user
        var username = $"testuser_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var password = "testpass123";

        var signupRequest = new
        {
            username = username,
            password = password,
            companyId = companyId,
            role = "User"
        };

        var signupResponse = await PostJsonAsync("/signup", signupRequest);
        signupResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "User signup should succeed");

        // 3. Login user
        var loginRequest = new
        {
            username = username,
            password = password
        };

        var loginResponse = await PostJsonAsync("/login", loginRequest);
        loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "User login should succeed");

        using var loginDoc = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = loginDoc.RootElement.GetProperty("token").GetString();
        token.Should().NotBeNullOrEmpty("Login token should be returned");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 4. Test authentication with token
        var authTestResponse = await _client.GetAsync("/test/user-only");
        authTestResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "Authentication test should succeed");

        // 5. Register device with deviceId 1
        var registerDeviceRequest = new { deviceId = 1 };

        var registerResponse = await PostJsonAsync("/Gateway/register", registerDeviceRequest);
        registerResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "Device registration should succeed");

        using var registerDoc = JsonDocument.Parse(await registerResponse.Content.ReadAsStringAsync());
        int deviceId = 0;
        if (registerDoc.RootElement.TryGetProperty("deviceId", out var deviceIdProp))
        {
            deviceId = deviceIdProp.GetInt32();
        }
        else
        {
            // Some responses may not return deviceId, handle that
            deviceId = 1; // fallback 
        }
        /*
        // 6. Post sensor data
        var sensorDataRequest = new
        {
            gatewayId = deviceId,
            temperatureCel = 22.5,
            humdityPct = 55.2
        };

        var sensorPostResponse = await PostJsonAsync("/Sensor", sensorDataRequest);
        sensorPostResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "Posting sensor data should succeed");
        */
    }

    private async Task<HttpResponseMessage> PostJsonAsync(string url, object data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _client.PostAsync(url, content);
    }
}
