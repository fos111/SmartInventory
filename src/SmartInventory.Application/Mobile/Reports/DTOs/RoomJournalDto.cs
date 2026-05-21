using System;
using System.Collections.Generic;

namespace SmartInventory.Application.Mobile.Reports.DTOs;

public class RoomJournalDto
{
    public string RoomCode { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<RoomJournalEntryDto> Entries { get; set; } = new();
}
