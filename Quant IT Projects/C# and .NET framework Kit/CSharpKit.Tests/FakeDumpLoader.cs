// Role : mimic a real data by smtg chosen, juste created in a second .net environment, to be tested by unitest.cs, but with a reference allowed between the two projects (here CSharpKit and CSharpKitTests) to give accees to the test envrionnement to IDumpLoader and PricingDumpE11.
// Test depends to the code, but not the opposite.

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
