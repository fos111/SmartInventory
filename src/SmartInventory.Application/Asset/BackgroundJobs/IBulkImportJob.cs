using System;
using System.Threading.Tasks;

namespace SmartInventory.Application.Asset.BackgroundJobs;

public interface IBulkImportJob
{
    Task RunAsync(string jobId, byte[] csvBytes, Guid userId);
}
