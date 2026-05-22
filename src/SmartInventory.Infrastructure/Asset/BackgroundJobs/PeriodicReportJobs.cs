using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartInventory.Application.Asset.BackgroundJobs;
using SmartInventory.Application.Asset.DTOs.Reports;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Caching;
using SmartInventory.Domain.Auth.Enums;

namespace SmartInventory.Infrastructure.Asset.BackgroundJobs;

public abstract class BasePeriodicReportJob
{
    protected readonly IReportingService Reporting;
    protected readonly IPdfReportService Pdf;
    protected readonly IBlobCacheService? Cache;

    protected BasePeriodicReportJob(
        IReportingService reportingService,
        IPdfReportService pdfService,
        IBlobCacheService? cacheService)
    {
        Reporting = reportingService;
        Pdf = pdfService;
        Cache = cacheService;
    }

    protected async Task CachePdfAsync(string key, byte[] pdfData, CancellationToken ct)
    {
        if (Cache == null || pdfData == null || pdfData.Length == 0) return;
        await Cache.SetAsync(key, pdfData, "application/pdf", ct);
    }
}

public class MonthlyReportJob( IReportingService reportingService, IPdfReportService pdfService, IBlobCacheService? cacheService = null)
    : BasePeriodicReportJob(reportingService, pdfService, cacheService), IMonthlyReportJob
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var departmentReport = await Reporting.GetDepartmentReportsAsync();
        var statusSummary = await Reporting.GetStatusSummaryAsync();
        var categoryStocktake = await Reporting.GetCategoryStocktakeAsync();
        var criticalIssues = await Reporting.GetCriticalIssuesAsync();

        if (Cache == null) return;

        if (departmentReport.Count > 0)
            await CachePdfAsync($"reports/monthly/{now:yyyy-MM}.pdf",
                await Pdf.GenerateDepartmentReportAsync(departmentReport, ct), ct);

        if (statusSummary.Count > 0)
            await CachePdfAsync($"reports/monthly/{now:yyyy-MM}-status.pdf",
                await Pdf.GenerateStatusSummaryAsync(statusSummary, ct), ct);

        if (categoryStocktake.Count > 0)
            await CachePdfAsync($"reports/monthly/{now:yyyy-MM}-categories.pdf",
                await Pdf.GenerateCategoryStocktakeAsync(categoryStocktake, ct), ct);

        if (criticalIssues.Count > 0)
            await CachePdfAsync($"reports/monthly/{now:yyyy-MM}-issues.pdf",
                await Pdf.GenerateCriticalIssuesAsync(criticalIssues, ct), ct);
    }
}

public class SemesterReportJob( IReportingService reportingService, IPdfReportService pdfService, IBlobCacheService? cacheService = null)
    : BasePeriodicReportJob(reportingService, pdfService, cacheService), ISemesterReportJob
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var semester = now.Month <= 6 ? "S1" : "S2";

        var departments = await Reporting.GetDepartmentReportsAsync();

        if (Cache == null) return;

        if (departments.Count > 0)
            await CachePdfAsync($"reports/semester/{now.Year}-{semester}.pdf",
                await Pdf.GenerateDepartmentReportAsync(departments, ct), ct);
    }
}

public class YearlyReportJob( IReportingService reportingService, IPdfReportService pdfService, IBlobCacheService? cacheService = null)
    : BasePeriodicReportJob(reportingService, pdfService, cacheService), IYearlyReportJob
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var departments = await Reporting.GetDepartmentReportsAsync();
        var pdf = await Pdf.GenerateDepartmentReportAsync(departments, ct);

        if (Cache == null) return;

        await CachePdfAsync($"reports/yearly/{now.Year}.pdf", pdf, ct);
    }
}


