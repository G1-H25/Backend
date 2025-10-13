using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;

public class AuthorizationServiceTests
{
    private AuthorizationService CreateServiceWithMocks(
        List<Dictionary<string, object>>? mockAdvancedResult)
    {
        var mockSqlGet = new Mock<ISqlGet>();
        mockSqlGet.Setup(x => x.FetchAsync(
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<IEnumerable<string>?>()))
        .ReturnsAsync((Dictionary<string, object>?)null); // You can adjust if needed

        var mockSqlGetAdvanced = new Mock<ISqlGetAdvanced>();
        mockSqlGetAdvanced.Setup(x => x.FetchWithJoinsAsync<Dictionary<string, object>>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<Dictionary<string, object>>(),
                null))
            .ReturnsAsync(mockAdvancedResult ?? new List<Dictionary<string, object>>());

        return new AuthorizationService(mockSqlGet.Object, mockSqlGetAdvanced.Object);
    }

    [Fact]
    public async Task HasAccessToDevice_ReturnsTrue_WhenUserIsOwner()
    {
        var service = CreateServiceWithMocks(new Dictionary<string, object>
        {
            { "OwnerId", 1 },
            { "CompanyId", 100 }
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
        var service = CreateServiceWithMocks(new Dictionary<string, object>
        {
            { "OwnerId", 2 },
            { "CompanyId", 100 }
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
        var service = CreateServiceWithMocks(new Dictionary<string, object>
        {
            { "OwnerId", 2 },
            { "CompanyId", 200 }
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
        var service = CreateServiceWithMocks(null); // Simulate not found

        var result = await service.HasAccessToDevice(
            userId: 1,
            role: "User",
            companyId: 100,
            gatewayId: 999);

        Assert.False(result);
    }
}
