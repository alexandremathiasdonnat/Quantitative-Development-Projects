using System.Threading.Tasks;
using DumpViewer.Core.Models;

namespace DumpViewer.Core.Services;

public interface IDumpLoader
{
    Task<PricingDump> LoadAsync(string path);
}
