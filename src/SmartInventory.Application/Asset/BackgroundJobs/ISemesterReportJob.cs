using System.Threading;
using System.Threading.Tasks;

namespace SmartInventory.Application.Asset.BackgroundJobs;

public interface ISemesterReportJob
{
    Task RunAsync(CancellationToken ct = default);
}
