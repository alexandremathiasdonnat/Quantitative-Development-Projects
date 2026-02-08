using System.IO;
using System.Threading.Tasks;
using DumpViewer.Core.ViewModels;
using DumpViewer.Core.Services;
using Xunit;

namespace DumpViewer.Tests;

public class MainViewModelTests
{
    [Fact]
    public async Task LoadFromPathAsync_ValidDump_PopulatesProperties()
    {
        var loader = new JsonDumpLoader();
        var vm = new MainViewModel(loader);

        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "DumpViewer.App", "SampleDumps", "sample_dump.json"));

        await vm.LoadFromPathAsync(path);

        Assert.Equal("Ready.", "Ready."); // sanity (optional)
        Assert.NotNull(vm.Dump);
        Assert.False(string.IsNullOrWhiteSpace(vm.ProductId));
        Assert.NotEqual("-", vm.Price);
        Assert.True(vm.Greeks.Count > 0);
        Assert.Contains("Loaded:", vm.StatusMessage);
        Assert.False(vm.IsLoading);
    }
}
