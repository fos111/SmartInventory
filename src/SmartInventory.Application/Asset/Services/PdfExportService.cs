using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartInventory.Application.Asset.Interfaces;

namespace SmartInventory.Application.Asset.Services;

public class PdfExportService : IExportService
{
    static PdfExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data, string filename)
    {
        throw new NotImplementedException("Use CsvExportService for CSV generation");
    }

    public Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, string reportTitle, string filename)
    {
        var records = data.ToList();
        if (!records.Any())
            return Task.FromResult(Array.Empty<byte>());

        var firstRecord = records.First();
        var properties = firstRecord.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var headers = properties.Select(p => p.Name).ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Text(reportTitle)
                    .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var _ in headers)
                            columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        foreach (var headerText in headers)
                        {
                            header.Cell().Element(c => c.DefaultTextStyle(x => x.Bold()))
                                .Background(Colors.Blue.Lighten3)
                                .Padding(5)
                                .Text(headerText);
                        }
                    });

                    foreach (var record in records)
                    {
                        foreach (var property in properties)
                        {
                            var value = property.GetValue(record)?.ToString() ?? string.Empty;
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(value);
                        }
                    }
                });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return Task.FromResult(stream.ToArray());
    }
}
