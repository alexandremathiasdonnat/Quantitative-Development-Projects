// Role : check a behavior with an asseretion on  FakeDumpLoader.cs

using System.Threading.Tasks;
using Xunit;

public class DumpLoaderTests
{
    [Fact]
    public async Task LoadAsync_ReturnsDump_WithPositivePrice()
    {
        // Arrange
        IDumpLoader loader = new FakeDumpLoader();

        // Act
        PricingDumpE11 dump = await loader.LoadAsync("ignored");

        // Assert
        Assert.True(dump.Price > 0);
    }
}
