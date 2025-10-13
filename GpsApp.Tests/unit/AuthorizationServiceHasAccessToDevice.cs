using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AuthorizationServiceTests
{
    [Fact]
    public async Task HasAccessToDevice_ReturnsTrue_WhenUserIsOwner()
    {
        // Arrange
        var mockSqlGet = new Mock<ISqlGet>();

        mockSqlGet.Setup(x => x.FetchAsync(
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<IEnumerable<string>?>()
        )).ReturnsAsync(new Dictionary<string, object>
        {
            { "OwnerId", 1 },
            { "CompanyId", 100 }
        });

        var service = new AuthorizationService(mockSqlGet.Object);

        // Act
        var result = await service.HasAccessToDevice(
            userId: 1,
            role: "User",
            companyId: 100,
            gatewayId: 123 // ✅ Now using int
        );

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasAccessToDevice_ReturnsTrue_WhenAdminFromSameCompany()
    {
        // Arrange
        var mockSqlGet = new Mock<ISqlGet>();

        mockSqlGet.Setup(x => x.FetchAsync(
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<IEnumerable<string>?>()))
        .ReturnsAsync(new Dictionary<string, object>
        {
            { "OwnerId", 2 },
            { "CompanyId", 100 }
        });

        var service = new AuthorizationService(mockSqlGet.Object);

        // Act
        var result = await service.HasAccessToDevice(
            userId: 1,
            role: "Admin",
            companyId: 100,
            gatewayId: 123
        );

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasAccessToDevice_ReturnsFalse_WhenUserNotOwner_Or_Admin()
    {
        // Arrange
        var mockSqlGet = new Mock<ISqlGet>();

        mockSqlGet.Setup(x => x.FetchAsync(
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<IEnumerable<string>?>()))
        .ReturnsAsync(new Dictionary<string, object>
        {
            { "OwnerId", 2 },
            { "CompanyId", 200 } // Different company
        });

        var service = new AuthorizationService(mockSqlGet.Object);

        // Act
        var result = await service.HasAccessToDevice(
            userId: 1,
            role: "User",
            companyId: 100,
            gatewayId: 123
        );

        // Assert
        Assert.False(result);
    }
    [Fact]
    public async Task HasAccessToDevice_ReturnsFalse_WhenGatewayDoesNotExist()
    {
        // Arrange
        var mockSqlGet = new Mock<ISqlGet>();

        mockSqlGet.Setup(x => x.FetchAsync(
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<IEnumerable<string>?>()))
        .ReturnsAsync((Dictionary<string, object>?)null); // Simulate not found

        var service = new AuthorizationService(mockSqlGet.Object);

        // Act
        var result = await service.HasAccessToDevice(
            userId: 1,
            role: "User",
            companyId: 100,
            gatewayId: 999
        );

        // Assert
        Assert.False(result);
    }

}
