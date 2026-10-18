using Microsoft.AspNetCore.Mvc.Testing;

namespace TripLink.IntegrationTests
{
    public class ApiSmokeTests
    {
        [Fact]
        public async Task Api_ShouldStartSuccessfully()
        {
            // Arrange
            await using var factory =
                new WebApplicationFactory<Program>();

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/swagger/index.html");

            // Assert
            Assert.NotNull(response);
        }
    }
}