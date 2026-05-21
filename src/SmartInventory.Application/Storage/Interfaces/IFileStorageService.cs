using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SmartInventory.Application.Storage.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(string container, string fileName, Stream fileStream, string contentType, CancellationToken ct = default);
    Task<bool> DeleteFileAsync(string fileUrl, CancellationToken ct = default);
}
