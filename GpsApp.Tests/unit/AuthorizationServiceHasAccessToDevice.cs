using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;

public class AuthorizationServiceTests
{
    private AuthorizationService CreateServiceWithMocks(
        List<Dictionary<string, object>>? mockResults)
    {
        var mockSqlGet = new Mock<ISqlGet>();
        mockSqlGet.Setup(x => x.FetchAsync(
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<IEnumerable<string>?>()))
        .ReturnsAsync(mockResults?.Count > 0 ? mockResults[0] : null);

        var mockSqlGetAdvanced = new Mock<ISqlGetAdvanced>();
        mockSqlGetAdvanced.Setup(x => x.FetchWithJoinsAsync<Dictionary<string, object>>(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<Dictionary<string, object>>(),
            null))
            .ReturnsAsync(mockResults ?? new List<Dictionary<string, object>>());

        return new AuthorizationService(mockSqlGet.Object, mockSqlGetAdvanced.Object);
    }

    [Fact]
    public async Task HasAccessToDevice_ReturnsTrue_WhenUserIsOwner()
    {
        var service = CreateServiceWithMocks(new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "OwnerId", 1 },
                { "CompanyId", 100 }
            }
        });

        var result = await service.HasAccessToDevice(
            userId: 1,
            role: "User",
            companyId: 100,
            gatewayId: 123);

        Assert.True(result);
    }

    [Fact]
    public async Task HasAccessToDevice_ReturnsTrue_WhenAdminFromSameCompany()
    {
        var service = CreateServiceWithMocks(new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "OwnerId", 2 },
                { "CompanyId", 100 }
            }
        });

        var result = await service.HasAccessToDevice(
            userId: 1,
            role: "Admin",
            companyId: 100,
            gatewayId: 123);

        Assert.True(result);
    }

    [Fact]
    public async Task HasAccessToDevice_ReturnsFalse_WhenUserNotOwner_Or_Admin()
    {
        var service = CreateServiceWithMocks(new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "OwnerId", 2 },
                { "CompanyId", 200 }
            }
        });

        var result = await service.HasAccessToDevice(
            userId: 1,
            role: "User",
            companyId: 100,
            gatewayId: 123);

        Assert.False(result);
    }

    [Fact]
    public async Task HasAccessToDevice_ReturnsFalse_WhenGatewayDoesNotExist()
    {
        var service = CreateServiceWithMocks(null); // Simulate no results

        var result = await service.HasAccessToDevice(
            userId: 1,
            role: "User",
            companyId: 100,
            gatewayId: 999);

        Assert.False(result);
    }
}
