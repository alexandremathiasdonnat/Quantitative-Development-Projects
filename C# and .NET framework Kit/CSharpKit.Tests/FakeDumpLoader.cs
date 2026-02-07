using System.Threading.Tasks;

public class FakeDumpLoader : IDumpLoader
{
    public Task<PricingDumpE11> LoadAsync(string path)
    {
        var dump = new PricingDumpE11
        {
            ProductId = "TEST",
            Price = 10m
        };

        return Task.FromResult(dump);
    }
}
