using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing.Exceptions;
using SmartInventory.Application.Asset.DTOs.Reports;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Mobile.Reports.DTOs;

namespace SmartInventory.Application.Asset.Services;

public class PdfReportService : IPdfReportService
{
    static PdfReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static readonly Color HeaderBg = Color.FromHex("#0f172a");
    private static readonly Color AccentBlue = Color.FromHex("#2563eb");
    private static readonly Color Slate400 = Color.FromHex("#94a3b8");
    private static readonly Color Slate600 = Color.FromHex("#475569");
    private static readonly Color Slate900 = Color.FromHex("#0f172a");

    // ── Document helpers ──────────────────────────────────────────

    private static void ComposeDefaultHeader(PageDescriptor page, string title, string subtitle)
    {
        page.Header().Height(22).Background(HeaderBg).Row(row =>
        {
            row.RelativeItem().PaddingLeft(12).PaddingTop(4)
                .Text($"EquipTrack | {subtitle}").FontSize(9).Bold().FontColor(Colors.White);
            row.RelativeItem().PaddingRight(12).PaddingTop(4).AlignRight()
                .Column(col =>
                {
                    col.Item().AlignRight().Text($"Generated: {DateTime.Now:MMMM dd, yyyy}").FontSize(7).FontColor(Slate400);
                    col.Item().AlignRight().Text($"Report: {title}").FontSize(7).FontColor(Slate400);
                });
        });
    }

    private static void ComposeDefaultFooter(PageDescriptor page, int totalPages)
    {
        page.Footer().Height(20).Background(Color.FromHex("#f8fafc")).Row(row =>
        {
            row.RelativeItem().PaddingLeft(12).PaddingTop(4)
                .Text("CONFIDENTIAL - EquipTrack Asset Management Platform").FontSize(7).FontColor(Slate400);
            row.RelativeItem().PaddingRight(12).PaddingTop(4).AlignRight()
                .Text(x =>
                {
                    x.Span("Page ").FontSize(7).FontColor(Slate400);
                    x.CurrentPageNumber().FontSize(7).FontColor(Slate400);
                    x.Span(" of ").FontSize(7).FontColor(Slate400);
                    x.TotalPages().FontSize(7).FontColor(Slate400);
                });
        });
    }

    private static Color StatusColor(string status)
    {
        return status switch
        {
            "Active" or "Operational" => Color.FromHex("#22c55e"),
            "Maintenance" => Color.FromHex("#f59e0b"),
            "CriticalIssue" or "Critical" => Color.FromHex("#ef4444"),
            "InStock" or "In Stock" => Color.FromHex("#06b6d4"),
            "Lost" => Color.FromHex("#6b7280"),
            "Retired" => Color.FromHex("#52525b"),
            _ => Slate600,
        };
    }

    private static string StatusLabel(string status)
    {
        return status switch
        {
            "Active" => "Active",
            "Maintenance" => "Maintenance",
            "CriticalIssue" => "Critical",
            "InStock" => "In Stock",
            "Lost" => "Lost",
            "Retired" => "Retired",
            _ => status,
        };
    }

    private static byte[] ToPdf(Document document)
    {
        using var stream = new MemoryStream();
        try
        {
            document.GeneratePdf(stream);
        }
        catch (DocumentLayoutException)
        {
            QuestPDF.Settings.EnableDebugging = true;
            document.GeneratePdf(stream);
        }
        return stream.ToArray();
    }

    // ── Maintenance & Status Reports ──────────────────────────────

    public Task<byte[]> GenerateMaintenanceForecastAsync(List<MaintenanceForecastDto> data, int days, CancellationToken ct = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);

                ComposeDefaultHeader(page, $"Maintenance Forecast ({days} Days)", "Industrial Asset Management");
                ComposeDefaultFooter(page, 0);

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Text($"Assets Due for Maintenance in the Next {days} Days")
                        .FontSize(12).Bold().FontColor(Slate900);
                    col.Item().Height(2).Background(AccentBlue);
                    col.Item().Text($"{data.Count} asset(s) require attention within the forecast window.")
                        .FontSize(8).FontColor(Slate600);

                    if (data.Count == 0) return;

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(22);
                            c.RelativeColumn(3);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.ConstantColumn(18);
                            c.RelativeColumn(1.5f);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("#").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Asset").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Category").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Room").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Days").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Due Date").FontSize(7).Bold().FontColor(Colors.White);
                        });

                        var idx = 1;
                        foreach (var item in data)
                        {
                            var bg = idx % 2 == 0 ? Colors.White : Color.FromHex("#f8fafc");
                            table.Cell().Background(bg).Padding(3)
                                .Text(idx.ToString()).FontSize(7).FontColor(Slate600);
                            table.Cell().Background(bg).Padding(3)
                                .Text(item.Name).FontSize(7).Bold().FontColor(Slate900);
                            table.Cell().Background(bg).Padding(3)
                                .Text(item.Category).FontSize(7).FontColor(Slate600);
                            table.Cell().Background(bg).Padding(3)
                                .Text(item.CurrentRoomCode).FontSize(7).FontColor(Slate600);
                            table.Cell().Background(bg).Padding(3)
                                .Text($"{item.DaysUntilDue}d").FontSize(7).Bold()
                                .FontColor(item.DaysUntilDue <= 7 ? Colors.Red.Medium : AccentBlue);
                            table.Cell().Background(bg).Padding(3)
                                .Text(item.MaintenanceDueDate?.ToString("MMM dd, yyyy") ?? "-")
                                .FontSize(7).FontColor(Slate600);
                            idx++;
                        }
                    });
                });
            });
        });

        return Task.FromResult(ToPdf(document));
    }

    public Task<byte[]> GenerateOverdueMaintenanceAsync(List<OverdueMaintenanceDto> data, CancellationToken ct = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);

                ComposeDefaultHeader(page, "Overdue Maintenance Report", "Industrial Asset Management");
                ComposeDefaultFooter(page, 0);

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Text("Overdue Maintenance").FontSize(12).Bold().FontColor(Slate900);
                    col.Item().Height(2).Background(Colors.Red.Medium);
                    col.Item().Text($"{data.Count} asset(s) are past their maintenance due date.")
                        .FontSize(8).FontColor(Colors.Red.Medium);

                    if (data.Count == 0) return;

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(22);
                            c.RelativeColumn(3);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.ConstantColumn(20);
                            c.RelativeColumn(1.5f);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("#").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Asset").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Category").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Room").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Overdue").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Due Date").FontSize(7).Bold().FontColor(Colors.White);
                        });

                        var idx = 1;
                        foreach (var item in data.OrderByDescending(a => a.DaysOverdue))
                        {
                            var bg = idx % 2 == 0 ? Colors.White : Color.FromHex("#fef2f2");
                            var overdueColor = item.DaysOverdue > 30 ? Colors.Red.Darken2 : Colors.Red.Medium;
                            table.Cell().Background(bg).Padding(3)
                                .Text(idx.ToString()).FontSize(7).FontColor(Slate600);
                            table.Cell().Background(bg).Padding(3)
                                .Text(item.Name).FontSize(7).Bold().FontColor(Slate900);
                            table.Cell().Background(bg).Padding(3)
                                .Text(item.Category).FontSize(7).FontColor(Slate600);
                            table.Cell().Background(bg).Padding(3)
                                .Text(item.CurrentRoomCode).FontSize(7).FontColor(Slate600);
                            table.Cell().Background(bg).Padding(3)
                                .Text($"{item.DaysOverdue}d").FontSize(7).Bold().FontColor(overdueColor);
                            table.Cell().Background(bg).Padding(3)
                                .Text(item.MaintenanceDueDate?.ToString("MMM dd, yyyy") ?? "-")
                                .FontSize(7).FontColor(Slate600);
                            idx++;
                        }
                    });
                });
            });
        });

        return Task.FromResult(ToPdf(document));
    }

    public Task<byte[]> GenerateCriticalIssuesAsync(List<CriticalIssueDto> data, CancellationToken ct = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);

                ComposeDefaultHeader(page, "Critical Issues Report", "Industrial Asset Management");
                ComposeDefaultFooter(page, 0);

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Text("Critical & Lost Assets").FontSize(12).Bold().FontColor(Slate900);
                    col.Item().Height(2).Background(Colors.Red.Medium);
                    col.Item().Text($"{data.Count} asset(s) require immediate attention.")
                        .FontSize(8).FontColor(Colors.Red.Medium);

                    if (data.Count == 0) return;

                    var critical = data.Where(a => a.Status == "CriticalIssue").ToList();
                    var lost = data.Where(a => a.Status == "Lost").ToList();

                    if (critical.Count > 0)
                    {
                        col.Item().PaddingTop(4).Text($"Critical Issues ({critical.Count})")
                            .FontSize(10).Bold().FontColor(Colors.Red.Darken2);
                        RenderAssetTable(col, critical);
                    }

                    if (lost.Count > 0)
                    {
                        col.Item().PaddingTop(4).Text($"Lost Assets ({lost.Count})")
                            .FontSize(10).Bold().FontColor(Colors.Orange.Darken2);
                        RenderAssetTable(col, lost);
                    }
                });
            });
        });

        return Task.FromResult(ToPdf(document));
    }

    private void RenderAssetTable(ColumnDescriptor col, List<CriticalIssueDto> items)
    {
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(22);
                c.RelativeColumn(3);
                c.RelativeColumn(1);
                c.RelativeColumn(1);
                c.RelativeColumn(1.5f);
            });

            table.Header(h =>
            {
                h.Cell().Background(HeaderBg).Padding(4)
                    .Text("#").FontSize(7).Bold().FontColor(Colors.White);
                h.Cell().Background(HeaderBg).Padding(4)
                    .Text("Asset").FontSize(7).Bold().FontColor(Colors.White);
                h.Cell().Background(HeaderBg).Padding(4)
                    .Text("Category").FontSize(7).Bold().FontColor(Colors.White);
                h.Cell().Background(HeaderBg).Padding(4)
                    .Text("Room").FontSize(7).Bold().FontColor(Colors.White);
                h.Cell().Background(HeaderBg).Padding(4)
                    .Text("Last Seen").FontSize(7).Bold().FontColor(Colors.White);
            });

            var idx = 1;
            foreach (var item in items)
            {
                var bg = idx % 2 == 0 ? Colors.White : Color.FromHex("#fff1f2");
                table.Cell().Background(bg).Padding(3)
                    .Text(idx.ToString()).FontSize(7).FontColor(Slate600);
                table.Cell().Background(bg).Padding(3)
                    .Text(item.Name).FontSize(7).Bold().FontColor(Slate900);
                table.Cell().Background(bg).Padding(3)
                    .Text(item.Category).FontSize(7).FontColor(Slate600);
                table.Cell().Background(bg).Padding(3)
                    .Text(item.CurrentRoomCode).FontSize(7).FontColor(Slate600);
                table.Cell().Background(bg).Padding(3)
                    .Text(item.LastSeen?.ToString("MMM dd, yyyy") ?? "N/A")
                    .FontSize(7).FontColor(Colors.Red.Medium);
                idx++;
            }
        });
    }

    public Task<byte[]> GenerateStatusSummaryAsync(List<StatusSummaryDto> data, CancellationToken ct = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);

                ComposeDefaultHeader(page, "Equipment Status Summary", "Industrial Asset Management");
                ComposeDefaultFooter(page, 0);

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Text("Equipment Status Distribution")
                        .FontSize(12).Bold().FontColor(Slate900);
                    col.Item().Height(2).Background(AccentBlue);

                    var total = data.Sum(d => d.Count);
                    col.Item().Text($"Total Assets: {total}  |  {data.Count(d => d.Status == "Active")} operational, {data.Count(d => d.Status == "Maintenance")} in maintenance")
                        .FontSize(8).FontColor(Slate600);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1);
                            c.ConstantColumn(15);
                            c.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Status").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4).AlignCenter()
                                .Text("Count").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Percentage").FontSize(7).Bold().FontColor(Colors.White);
                        });

                        foreach (var item in data)
                        {
                            var bg = item.Status == "Active" ? Color.FromHex("#f0fdf4")
                                : item.Status == "CriticalIssue" ? Color.FromHex("#fef2f2")
                                : item.Status == "Lost" ? Color.FromHex("#f8fafc")
                                : Colors.White;

                            table.Cell().Background(bg).Padding(4)
                                .Text(StatusLabel(item.Status)).FontSize(8).Bold()
                                .FontColor(StatusColor(item.Status));
                            table.Cell().Background(bg).Padding(4).AlignCenter()
                                .Text(item.Count.ToString()).FontSize(8).Bold().FontColor(Slate900);
                            table.Cell().Background(bg).Padding(4)
                                .Text($"{item.Percentage}%").FontSize(8).FontColor(Slate600);
                        }
                    });
                });
            });
        });

        return Task.FromResult(ToPdf(document));
    }

    // ── Location-Based Reports ────────────────────────────────────

    public Task<byte[]> GenerateZoneInventoryAsync(List<ZoneInventoryDto> data, CancellationToken ct = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);

                ComposeDefaultHeader(page, "Zone Inventory Report", "Industrial Asset Management");
                ComposeDefaultFooter(page, 0);

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Text("Zone & Building Inventory").FontSize(12).Bold().FontColor(Slate900);
                    col.Item().Height(2).Background(AccentBlue);
                    col.Item().Text($"{data.Count} rooms across all zones.").FontSize(8).FontColor(Slate600);

                    if (data.Count == 0) return;

                    foreach (var zoneGroup in data.GroupBy(d => d.ZoneName))
                    {
                        col.Item().PaddingTop(4).Text($"Zone: {zoneGroup.Key}")
                            .FontSize(10).Bold().FontColor(Slate900);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1.5f);
                                c.ConstantColumn(14);
                                c.ConstantColumn(14);
                                c.ConstantColumn(14);
                                c.ConstantColumn(14);
                                c.ConstantColumn(14);
                                c.RelativeColumn(1);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background(HeaderBg).Padding(3)
                                    .Text("Room").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).AlignCenter()
                                    .Text("Total").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).AlignCenter()
                                    .Text("Active").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).AlignCenter()
                                    .Text("Maint").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).AlignCenter()
                                    .Text("Crit").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).AlignCenter()
                                    .Text("Stock").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3)
                                    .Text("Building").FontSize(6).Bold().FontColor(Colors.White);
                            });

                            var idx = 1;
                            foreach (var item in zoneGroup)
                            {
                                var bg = idx % 2 == 0 ? Colors.White : Color.FromHex("#f8fafc");
                                table.Cell().Background(bg).Padding(3)
                                    .Text(item.RoomCode).FontSize(6).Bold().FontColor(Slate900);
                                table.Cell().Background(bg).Padding(3).AlignCenter()
                                    .Text(item.TotalAssets.ToString()).FontSize(6).FontColor(Slate900);
                                table.Cell().Background(bg).Padding(3).AlignCenter()
                                    .Text(item.ActiveCount.ToString()).FontSize(6).FontColor(Colors.Green.Medium);
                                table.Cell().Background(bg).Padding(3).AlignCenter()
                                    .Text(item.MaintenanceCount.ToString()).FontSize(6).FontColor(Colors.Orange.Medium);
                                table.Cell().Background(bg).Padding(3).AlignCenter()
                                    .Text(item.CriticalCount.ToString()).FontSize(6).FontColor(Colors.Red.Medium);
                                table.Cell().Background(bg).Padding(3).AlignCenter()
                                    .Text(item.InStockCount.ToString()).FontSize(6).FontColor(Color.FromHex("#06b6d4"));
                                table.Cell().Background(bg).Padding(3)
                                    .Text(item.BuildingName).FontSize(6).FontColor(Slate600);
                                idx++;
                            }
                        });
                    }
                });
            });
        });

        return Task.FromResult(ToPdf(document));
    }

    public Task<byte[]> GenerateBuildingStocktakeAsync(List<BuildingStocktakeDto> data, CancellationToken ct = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);

                ComposeDefaultHeader(page, "Building Stocktake Report", "Industrial Asset Management");
                ComposeDefaultFooter(page, 0);

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Text("Building Stocktake").FontSize(12).Bold().FontColor(Slate900);
                    col.Item().Height(2).Background(AccentBlue);

                    if (data.Count == 0) return;

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.ConstantColumn(14);
                            c.ConstantColumn(16);
                            c.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Building").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4).AlignCenter()
                                .Text("Floors").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4).AlignCenter()
                                .Text("Assets").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Categories").FontSize(7).Bold().FontColor(Colors.White);
                        });

                        var idx = 1;
                        foreach (var item in data)
                        {
                            var bg = idx % 2 == 0 ? Colors.White : Color.FromHex("#f8fafc");
                            table.Cell().Background(bg).Padding(4)
                                .Text(item.BuildingName).FontSize(7).Bold().FontColor(Slate900);
                            table.Cell().Background(bg).Padding(4).AlignCenter()
                                .Text(item.FloorCount.ToString()).FontSize(7).FontColor(Slate600);
                            table.Cell().Background(bg).Padding(4).AlignCenter()
                                .Text(item.TotalAssets.ToString()).FontSize(7).Bold().FontColor(AccentBlue);
                            table.Cell().Background(bg).Padding(4)
                                .Text(string.Join(", ", item.Categories)).FontSize(6).FontColor(Slate600);
                            idx++;
                        }
                    });
                });
            });
        });

        return Task.FromResult(ToPdf(document));
    }

    public Task<byte[]> GenerateRoomAuditAsync(RoomAuditDto data, CancellationToken ct = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);

                ComposeDefaultHeader(page, $"Room Audit - {data.RoomCode}", "Industrial Asset Management");
                ComposeDefaultFooter(page, 0);

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Room: {data.RoomCode}").FontSize(14).Bold().FontColor(Slate900);
                            c.Item().Text($"{data.ZoneName ?? "N/A"} / {data.BuildingName ?? "N/A"}")
                                .FontSize(8).FontColor(Slate600);
                        });
                        row.ConstantItem(100).Column(c =>
                        {
                            c.Item().AlignRight().Text($"{data.Assets.Count} assets")
                                .FontSize(14).Bold().FontColor(AccentBlue);
                        });
                    });

                    col.Item().Height(2).Background(AccentBlue);

                    if (data.Assets.Count == 0) return;

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(22);
                            c.RelativeColumn(3);
                            c.RelativeColumn(1.5f);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1.5f);
                            c.RelativeColumn(1.5f);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("#").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Asset").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Category").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Status").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("RFID Tag").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Last Seen").FontSize(7).Bold().FontColor(Colors.White);
                        });

                        var idx = 1;
                        foreach (var asset in data.Assets)
                        {
                            var bg = idx % 2 == 0 ? Colors.White : Color.FromHex("#f8fafc");
                            table.Cell().Background(bg).Padding(3)
                                .Text(idx.ToString()).FontSize(7).FontColor(Slate600);
                            table.Cell().Background(bg).Padding(3)
                                .Text(asset.Name).FontSize(7).Bold().FontColor(Slate900);
                            table.Cell().Background(bg).Padding(3)
                                .Text(asset.Category).FontSize(7).FontColor(Slate600);
                            table.Cell().Background(bg).Padding(3)
                                .Text(StatusLabel(asset.Status)).FontSize(7).Bold()
                                .FontColor(StatusColor(asset.Status));
                            table.Cell().Background(bg).Padding(3)
                                .Text(asset.RfidTagId ?? "-").FontSize(7).FontColor(Slate600);
                            table.Cell().Background(bg).Padding(3)
                                .Text(asset.LastSeen?.ToString("MMM dd, yyyy") ?? "N/A")
                                .FontSize(7).FontColor(Slate600);
                            idx++;
                        }
                    });
                });
            });
        });

        return Task.FromResult(ToPdf(document));
    }

    public Task<byte[]> GenerateEmptyRoomsAsync(List<EmptyRoomDto> data, CancellationToken ct = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);

                ComposeDefaultHeader(page, "Empty / Underutilized Rooms", "Industrial Asset Management");
                ComposeDefaultFooter(page, 0);

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Text("Empty & Underutilized Rooms").FontSize(12).Bold().FontColor(Slate900);
                    col.Item().Height(2).Background(Colors.Orange.Medium);
                    col.Item().Text($"{data.Count} room(s) identified with low asset occupancy.")
                        .FontSize(8).FontColor(Colors.Orange.Darken2);

                    if (data.Count == 0) return;

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(22);
                            c.RelativeColumn(1.5f);
                            c.RelativeColumn(1.5f);
                            c.RelativeColumn(1.5f);
                            c.ConstantColumn(14);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("#").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Room").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Zone").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Building").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4).AlignCenter()
                                .Text("Assets").FontSize(7).Bold().FontColor(Colors.White);
                        });

                        var idx = 1;
                        foreach (var item in data)
                        {
                            var bg = idx % 2 == 0 ? Colors.White : Color.FromHex("#fff7ed");
                            table.Cell().Background(bg).Padding(3)
                                .Text(idx.ToString()).FontSize(7).FontColor(Slate600);
                            table.Cell().Background(bg).Padding(3)
                                .Text(item.RoomCode).FontSize(7).Bold().FontColor(Slate900);
                            table.Cell().Background(bg).Padding(3)
                                .Text(item.ZoneName ?? "-").FontSize(7).FontColor(Slate600);
                            table.Cell().Background(bg).Padding(3)
                                .Text(item.BuildingName ?? "-").FontSize(7).FontColor(Slate600);
                            table.Cell().Background(bg).Padding(3).AlignCenter()
                                .Text(item.AssetCount.ToString()).FontSize(7).Bold().FontColor(Colors.Orange.Darken2);
                            idx++;
                        }
                    });
                });
            });
        });

        return Task.FromResult(ToPdf(document));
    }

    // ── Audit & Compliance Reports ────────────────────────────────

    public Task<byte[]> GenerateLocationDiscrepanciesAsync(List<LocationDiscrepancyDto> data, CancellationToken ct = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);

                ComposeDefaultHeader(page, "Location Discrepancy Report", "Industrial Asset Management");
                ComposeDefaultFooter(page, 0);

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Text("IoT Location Mismatches").FontSize(12).Bold().FontColor(Slate900);
                    col.Item().Height(2).Background(Colors.Orange.Medium);
                    col.Item().Text($"{data.Count} asset(s) have mismatch between user-set and IoT-detected location.")
                        .FontSize(8).FontColor(Colors.Orange.Darken2);

                    if (data.Count == 0) return;

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(22);
                            c.RelativeColumn(3);
                            c.RelativeColumn(1.5f);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("#").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Asset").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("Category").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("User Location").FontSize(7).Bold().FontColor(Colors.White);
                            h.Cell().Background(HeaderBg).Padding(4)
                                .Text("IoT Location").FontSize(7).Bold().FontColor(Colors.White);
                        });

                        var idx = 1;
                        foreach (var item in data)
                        {
                            var bg = idx % 2 == 0 ? Colors.White : Color.FromHex("#fff7ed");
                            table.Cell().Background(bg).Padding(3)
                                .Text(idx.ToString()).FontSize(7).FontColor(Slate600);
                            table.Cell().Background(bg).Padding(3)
                                .Text(item.Name).FontSize(7).Bold().FontColor(Slate900);
                            table.Cell().Background(bg).Padding(3)
                                .Text(item.Category).FontSize(7).FontColor(Slate600);
                            table.Cell().Background(bg).Padding(3)
                                .Text(item.CurrentRoomCode).FontSize(7).FontColor(Colors.Red.Medium);
                            table.Cell().Background(bg).Padding(3)
                                .Text(item.DetectedRoomCode ?? "N/A").FontSize(7).FontColor(AccentBlue);
                            idx++;
                        }
                    });
                });
            });
        });

        return Task.FromResult(ToPdf(document));
    }

    public Task<byte[]> GenerateCategoryStocktakeAsync(List<CategoryStocktakeDto> data, CancellationToken ct = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);

                ComposeDefaultHeader(page, "Category Stocktake Report", "Industrial Asset Management");
                ComposeDefaultFooter(page, 0);

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Text("Category Stocktake").FontSize(12).Bold().FontColor(Slate900);
                    col.Item().Height(2).Background(AccentBlue);

                    var total = data.Sum(d => d.Count);
                    col.Item().Text($"{data.Count} categories, {total} total assets.")
                        .FontSize(8).FontColor(Slate600);

                    if (data.Count == 0) return;

                    foreach (var cat in data)
                    {
                        col.Item().PaddingTop(4).Text($"{cat.Category}  ({cat.Count} assets, {cat.Percentage}%)")
                            .FontSize(9).Bold().FontColor(Slate900);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1);
                                c.ConstantColumn(12);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background(HeaderBg).Padding(3)
                                    .Text("Status").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).AlignCenter()
                                    .Text("Count").FontSize(6).Bold().FontColor(Colors.White);
                            });

                            foreach (var status in cat.StatusBreakdown)
                            {
                                table.Cell().Padding(3)
                                    .Text(StatusLabel(status.Status)).FontSize(7).Bold()
                                    .FontColor(StatusColor(status.Status));
                                table.Cell().Padding(3).AlignCenter()
                                    .Text(status.Count.ToString()).FontSize(7).FontColor(Slate900);
                            }
                        });
                    }
                });
            });
        });

        return Task.FromResult(ToPdf(document));
    }

    // ── Executive Reports ─────────────────────────────────────────

    public Task<byte[]> GenerateDepartmentReportAsync(List<DepartmentReportDto> data, CancellationToken ct = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);

                ComposeDefaultHeader(page, "Department Report", "Industrial Asset Management");
                ComposeDefaultFooter(page, 0);

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Text("Department Performance Summary")
                        .FontSize(12).Bold().FontColor(Slate900);
                    col.Item().Height(2).Background(AccentBlue);

                    if (data.Count == 0) return;

                    foreach (var dept in data)
                    {
                        col.Item().Padding(8).Border(1).BorderColor(Color.FromHex("#e2e8f0"))
                            .Background(Color.FromHex("#f8fafc")).Column(c =>
                        {
                            c.Spacing(4);
                            c.Item().Text(dept.ZoneName).FontSize(10).Bold().FontColor(Slate900);

                            c.Item().Row(row =>
                            {
                                row.RelativeItem().Background(Colors.White).Padding(4).Border(1)
                                    .BorderColor(Color.FromHex("#e2e8f0")).Column(card =>
                                    {
                                        card.Item().AlignCenter().Text(dept.TotalAssets.ToString())
                                            .FontSize(16).Bold().FontColor(AccentBlue);
                                        card.Item().AlignCenter().Text("Total Assets").FontSize(7).FontColor(Slate600);
                                    });
                                row.RelativeItem().Background(Colors.White).Padding(4).Border(1)
                                    .BorderColor(Color.FromHex("#e2e8f0")).Column(card =>
                                    {
                                        card.Item().AlignCenter().Text($"{dept.AvailabilityRate}%")
                                            .FontSize(16).Bold()
                                            .FontColor(dept.AvailabilityRate >= 80 ? Colors.Green.Medium : Colors.Orange.Medium);
                                        card.Item().AlignCenter().Text("Availability").FontSize(7).FontColor(Slate600);
                                    });
                                row.RelativeItem().Background(Colors.White).Padding(4).Border(1)
                                    .BorderColor(Color.FromHex("#e2e8f0")).Column(card =>
                                    {
                                        card.Item().AlignCenter().Text(dept.CriticalCount.ToString())
                                            .FontSize(16).Bold().FontColor(Colors.Red.Medium);
                                        card.Item().AlignCenter().Text("Critical").FontSize(7).FontColor(Slate600);
                                    });
                                row.RelativeItem().Background(Colors.White).Padding(4).Border(1)
                                    .BorderColor(Color.FromHex("#e2e8f0")).Column(card =>
                                    {
                                        card.Item().AlignCenter().Text(dept.MaintenanceCount.ToString())
                                            .FontSize(16).Bold().FontColor(Colors.Orange.Medium);
                                        card.Item().AlignCenter().Text("In Maint.").FontSize(7).FontColor(Slate600);
                                    });
                                row.RelativeItem().Background(Colors.White).Padding(4).Border(1)
                                    .BorderColor(Color.FromHex("#e2e8f0")).Column(card =>
                                    {
                                        card.Item().AlignCenter().Text(dept.InStockCount.ToString())
                                            .FontSize(16).Bold().FontColor(Color.FromHex("#06b6d4"));
                                        card.Item().AlignCenter().Text("In Stock").FontSize(7).FontColor(Slate600);
                                    });
                            });
                        });
                    }
                });
            });
        });

        return Task.FromResult(ToPdf(document));
    }

    public Task<byte[]> GenerateRoomJournalAsync(RoomJournalDto data, CancellationToken ct = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);

                ComposeDefaultHeader(page, "Room Journal", "Industrial Asset Management");
                ComposeDefaultFooter(page, 0);

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Room: {data.RoomCode}").FontSize(14).Bold().FontColor(Slate900);
                            c.Item().Text($"{data.RoomName}").FontSize(10).FontColor(Slate600);
                            if (!string.IsNullOrEmpty(data.DepartmentName))
                                c.Item().Text($"Department: {data.DepartmentName}").FontSize(9).FontColor(Slate600);
                        });

                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            if (data.FromDate.HasValue)
                                c.Item().Text($"From: {data.FromDate:yyyy-MM-dd}").FontSize(8).FontColor(Slate400);
                            if (data.ToDate.HasValue)
                                c.Item().Text($"To: {data.ToDate:yyyy-MM-dd}").FontSize(8).FontColor(Slate400);
                        });
                    });

                    col.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Slate400);

                    col.Item().Text($"Total Entries: {data.Entries.Count}").FontSize(10).Bold().FontColor(Slate900);

                    if (data.Entries.Count > 0)
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(HeaderBg).Padding(4)
                                    .Text("Asset Tag").FontSize(8).Bold().FontColor(Colors.White);
                                header.Cell().Background(HeaderBg).Padding(4)
                                    .Text("Asset Name").FontSize(8).Bold().FontColor(Colors.White);
                                header.Cell().Background(HeaderBg).Padding(4)
                                    .Text("Action").FontSize(8).Bold().FontColor(Colors.White);
                                header.Cell().Background(HeaderBg).Padding(4)
                                    .Text("From").FontSize(8).Bold().FontColor(Colors.White);
                                header.Cell().Background(HeaderBg).Padding(4)
                                    .Text("To").FontSize(8).Bold().FontColor(Colors.White);
                                header.Cell().Background(HeaderBg).Padding(4)
                                    .Text("By").FontSize(8).Bold().FontColor(Colors.White);
                                header.Cell().Background(HeaderBg).Padding(4)
                                    .Text("Date").FontSize(8).Bold().FontColor(Colors.White);
                            });

                            foreach (var entry in data.Entries)
                            {
                                var bgColor = (data.Entries.IndexOf(entry) % 2 == 0)
                                    ? Colors.White
                                    : Color.FromHex("#f8fafc");

                                table.Cell().Background(bgColor).Padding(3)
                                    .Text(entry.AssetTag).FontSize(7).FontColor(Slate900);
                                table.Cell().Background(bgColor).Padding(3)
                                    .Text(entry.AssetName).FontSize(7).FontColor(Slate600);
                                table.Cell().Background(bgColor).Padding(3)
                                    .Text(entry.Action).FontSize(7).FontColor(Slate600);
                                table.Cell().Background(bgColor).Padding(3)
                                    .Text(entry.OldValue ?? "-").FontSize(7).FontColor(Slate600);
                                table.Cell().Background(bgColor).Padding(3)
                                    .Text(entry.NewValue ?? "-").FontSize(7).FontColor(Slate600);
                                table.Cell().Background(bgColor).Padding(3)
                                    .Text(entry.ChangedByName).FontSize(7).FontColor(Slate600);
                                table.Cell().Background(bgColor).Padding(3)
                                    .Text(entry.ChangedAt.ToString("yyyy-MM-dd HH:mm")).FontSize(7).FontColor(Slate600);
                            }
                        });
                    }
                });
            });
        });

        return Task.FromResult(ToPdf(document));
    }

    public Task<byte[]> GenerateLocationReportAsync(LocationComprehensiveReportDto data, CancellationToken ct = default)
    {
        var scopeLabel = data.Scope switch
        {
            "zone" => "Zone (Department)",
            "building" => "Building",
            "room" => "Room",
            _ => data.Scope
        };

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(25);

                ComposeDefaultHeader(page, $"{scopeLabel} Report - {data.ScopeName}", "Industrial Asset Management");
                ComposeDefaultFooter(page, 0);

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"{scopeLabel}: {data.ScopeName}").FontSize(14).Bold().FontColor(Slate900);
                            c.Item().Text(BuildBreadcrumb(data.Hierarchy)).FontSize(8).FontColor(Slate600);
                        });
                        row.ConstantItem(100).Column(c =>
                        {
                            c.Item().AlignRight().Text($"{data.TotalAssets} assets").FontSize(14).Bold().FontColor(AccentBlue);
                        });
                    });

                    col.Item().Height(2).Background(AccentBlue);

                    col.Item().PaddingTop(4).Text($"Current Assets ({data.CurrentAssets.Count})")
                        .FontSize(12).Bold().FontColor(Slate900);

                    if (data.CurrentAssets.Count > 0)
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(22);
                                c.RelativeColumn(1.5f);
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1.5f);
                                c.RelativeColumn(1.5f);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background(HeaderBg).Padding(3).Text("#").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).Text("Tag").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).Text("Name").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).Text("Category").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).Text("Status").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).Text("S/N").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).Text("RFID").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).Text("Last Seen").FontSize(6).Bold().FontColor(Colors.White);
                            });

                            var idx = 1;
                            foreach (var asset in data.CurrentAssets)
                            {
                                var bg = idx % 2 == 0 ? Colors.White : Color.FromHex("#f8fafc");
                                table.Cell().Background(bg).Padding(3).Text(idx.ToString()).FontSize(6).FontColor(Slate600);
                                table.Cell().Background(bg).Padding(3).Text(asset.AssetTag).FontSize(6).Bold().FontColor(Slate900);
                                table.Cell().Background(bg).Padding(3).Text(asset.Name).FontSize(6).FontColor(Slate600);
                                table.Cell().Background(bg).Padding(3).Text(asset.Category).FontSize(6).FontColor(Slate600);
                                table.Cell().Background(bg).Padding(3).Text(StatusLabel(asset.Status.ToString())).FontSize(6).Bold().FontColor(StatusColor(asset.Status.ToString()));
                                table.Cell().Background(bg).Padding(3).Text(asset.SerialNumber ?? "-").FontSize(6).FontColor(Slate600);
                                table.Cell().Background(bg).Padding(3).Text(asset.RfidTagId ?? "-").FontSize(6).FontColor(Slate600);
                                table.Cell().Background(bg).Padding(3).Text(asset.LastSeen?.ToString("MMM dd, yyyy") ?? "N/A").FontSize(6).FontColor(Slate600);
                                idx++;
                            }
                        });
                    }

                    if (data.History.Count > 0)
                    {
                        col.Item().PaddingTop(8).Text($"Activity History ({data.History.Count})")
                            .FontSize(12).Bold().FontColor(Slate900);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1.5f);
                                c.RelativeColumn(1.5f);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1.5f);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background(HeaderBg).Padding(3).Text("Asset").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).Text("Event").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).Text("From").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).Text("To").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).Text("By").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).Text("Source").FontSize(6).Bold().FontColor(Colors.White);
                                h.Cell().Background(HeaderBg).Padding(3).Text("Date").FontSize(6).Bold().FontColor(Colors.White);
                            });

                            var hidx = 1;
                            foreach (var entry in data.History)
                            {
                                var bg = hidx % 2 == 0 ? Colors.White : Color.FromHex("#f8fafc");
                                table.Cell().Background(bg).Padding(3).Text(entry.AssetTag).FontSize(6).FontColor(Slate900);
                                table.Cell().Background(bg).Padding(3).Text(entry.EventType).FontSize(6).Bold().FontColor(AccentBlue);
                                table.Cell().Background(bg).Padding(3).Text(entry.OldValue ?? "-").FontSize(6).FontColor(Slate600);
                                table.Cell().Background(bg).Padding(3).Text(entry.NewValue ?? "-").FontSize(6).FontColor(Slate600);
                                table.Cell().Background(bg).Padding(3).Text(entry.ChangedBy).FontSize(6).FontColor(Slate600);
                                table.Cell().Background(bg).Padding(3).Text(entry.Source).FontSize(6).FontColor(Slate600);
                                table.Cell().Background(bg).Padding(3).Text(entry.Timestamp.ToString("yyyy-MM-dd HH:mm")).FontSize(6).FontColor(Slate600);
                                hidx++;
                            }
                        });
                    }

                    if (data.CurrentAssets.Count == 0 && data.History.Count == 0)
                    {
                        col.Item().Padding(20).AlignCenter()
                            .Text("No assets or activity found for this location.").FontSize(10).FontColor(Slate400);
                    }
                });
            });
        });

        return Task.FromResult(ToPdf(document));
    }

    private static string BuildBreadcrumb(LocationHierarchyInfo h)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(h.SiteName)) parts.Add(h.SiteName);
        if (!string.IsNullOrEmpty(h.ZoneName)) parts.Add(h.ZoneName);
        if (!string.IsNullOrEmpty(h.BuildingName)) parts.Add(h.BuildingName);
        if (h.FloorLevel.HasValue) parts.Add($"Level {h.FloorLevel}");
        if (!string.IsNullOrEmpty(h.RoomName)) parts.Add($"{h.RoomName} ({h.RoomCode})");
        else if (!string.IsNullOrEmpty(h.RoomCode)) parts.Add(h.RoomCode);
        return parts.Count > 0 ? string.Join(" > ", parts) : "Location";
    }
}
