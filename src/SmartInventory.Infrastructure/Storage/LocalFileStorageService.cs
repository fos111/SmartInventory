using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Storage.Interfaces;

namespace SmartInventory.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(string basePath)
    {
        _basePath = basePath;
    }

    public async Task<string> SaveFileAsync(
        string container, string fileName, Stream fileStream, string contentType, CancellationToken ct = default)
    {
        var uploadsDir = Path.Combine(_basePath, "uploads", container);
        Directory.CreateDirectory(uploadsDir);

        var filePath = Path.Combine(uploadsDir, fileName);
        await using var stream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(stream, ct);

        return $"/uploads/{container}/{fileName}";
    }

    public Task<bool> DeleteFileAsync(string fileUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return Task.FromResult(false);

        var relativePath = fileUrl.TrimStart('/');
        var filePath = Path.Combine(_basePath, relativePath);

        if (!File.Exists(filePath))
            return Task.FromResult(false);

        File.Delete(filePath);
        return Task.FromResult(true);
    }
}
