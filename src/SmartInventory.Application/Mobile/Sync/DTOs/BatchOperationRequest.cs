using System.Collections.Generic;

namespace SmartInventory.Application.Mobile.Sync.DTOs;

public class BatchOperationRequest
{
    public List<BatchAssetOperationDto> Operations { get; set; } = new();
}
