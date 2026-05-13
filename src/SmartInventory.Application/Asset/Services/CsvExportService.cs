using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using SmartInventory.Application.Asset.Interfaces;

namespace SmartInventory.Application.Asset.Services;

public class CsvExportService : IExportService
{
    public Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data, string filename)
    {
        var records = data.ToList();
        if (!records.Any())
            return Task.FromResult(Array.Empty<byte>());

        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        });

        var firstRecord = records.First();
        var properties = firstRecord.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            csvWriter.WriteField(property.Name);
        }
        csvWriter.NextRecord();

        foreach (var record in records)
        {
            foreach (var property in properties)
            {
                var value = property.GetValue(record);
                csvWriter.WriteField(value?.ToString() ?? string.Empty);
            }
            csvWriter.NextRecord();
        }

        streamWriter.Flush();
        return Task.FromResult(memoryStream.ToArray());
    }

    public Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, string reportTitle, string filename)
    {
        throw new NotImplementedException("Use PdfExportService for PDF generation");
    }
}
