using System.IO;
using System.Threading.Tasks;
using DumpViewer.Core.Services;
using Xunit;

namespace DumpViewer.Tests;

public class JsonDumpLoaderTests
{
    [Fact]
    public async Task LoadAsync_ValidJson_ReturnsDump()
    {
        var loader = new JsonDumpLoader();
        var path = Path.Combine("..", "..", "..", "..", "DumpViewer.App", "SampleDumps", "sample_dump.json");

        var dump = await loader.LoadAsync(path);

        Assert.False(string.IsNullOrWhiteSpace(dump.ProductId));
        Assert.True(dump.Price != 0m);
    }
}
