using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartInventory.Application.Asset.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data, string filename);
    Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, string reportTitle, string filename);
}
