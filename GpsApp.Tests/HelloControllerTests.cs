using Xunit;
using GpsApp.Controllers; // namespace
using Microsoft.AspNetCore.Mvc;

namespace GpsApp.Tests
{
    public class HelloControllerTests
    {
        [Fact]
        public void Get_ReturnsOkResult_WithCorrectMessage()
        {
            // Arrange
            var controller = new HelloController();

            // Act
            var result = controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Hello from the ASP.NET Core API!", okResult.Value);
        }
    }
}
