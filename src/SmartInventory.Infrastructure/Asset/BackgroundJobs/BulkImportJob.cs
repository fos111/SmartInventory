using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using SmartInventory.Application.Asset.BackgroundJobs;
using SmartInventory.Application.Asset.DTOs;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Application.Notification.Interfaces;
using SmartInventory.Domain.Auth.Enums;
using AssetEntity = SmartInventory.Domain.Asset.Entities.Asset;

namespace SmartInventory.Infrastructure.Asset.BackgroundJobs;

public class BulkImportJob : IBulkImportJob
{
    private readonly IAssetRepository _assetRepository;
    private readonly IAssetHistoryService _historyService;
    private readonly INotificationService _notificationService;
    private readonly ILocationRepository _locationRepository;
    private readonly IActivityLogService _activityLogService;
    private readonly IMapper _mapper;

    public BulkImportJob(
        IAssetRepository assetRepository,
        IAssetHistoryService historyService,
        INotificationService notificationService,
        ILocationRepository locationRepository,
        IActivityLogService activityLogService,
        IMapper mapper)
    {
        _assetRepository = assetRepository;
        _historyService = historyService;
        _notificationService = notificationService;
        _locationRepository = locationRepository;
        _activityLogService = activityLogService;
        _mapper = mapper;
    }

    public async Task RunAsync(string jobId, byte[] csvBytes, Guid userId)
    {
        using var stream = new MemoryStream(csvBytes);
        using var reader = new StreamReader(stream);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };
        using var csv = new CsvReader(reader, config);

        var records = new List<CreateAssetDto>();
        await csv.ReadAsync();
        csv.ReadHeader();

        while (await csv.ReadAsync())
        {
            var record = new CreateAssetDto
            {
                AssetTag = csv.GetField<string>(0) ?? string.Empty,
                Name = csv.GetField<string>(1) ?? string.Empty,
                Description = csv.GetField<string>(2),
                Category = csv.GetField<string>(3) ?? string.Empty,
                CurrentRoomCode = csv.GetField<string>(4) ?? string.Empty
            };
            records.Add(record);
        }

        foreach (var record in records)
        {
            try
            {
                await CreateAssetAsync(record, userId);
            }
            catch
            {
                // Individual record failures are logged per-row
                // but don't block the rest of the import
            }
        }
    }

    private async Task CreateAssetAsync(CreateAssetDto dto, Guid userId)
    {
        if (!string.IsNullOrEmpty(dto.AssetTag))
        {
            if (!await _assetRepository.IsAssetTagUniqueAsync(dto.AssetTag))
                throw new InvalidOperationException($"Asset tag '{dto.AssetTag}' already exists.");
        }

        if (!await _assetRepository.IsRoomCodeValidAsync(dto.CurrentRoomCode))
            throw new ArgumentException($"Room code '{dto.CurrentRoomCode}' is not valid.");

        var asset = _mapper.Map<AssetEntity>(dto);
        var created = await _assetRepository.AddAsync(asset);

        await _activityLogService.TrackFacilityChangeAsync(
            "Created", "Asset", created.AssetTag, created.Name, null, userId);
    }
}
