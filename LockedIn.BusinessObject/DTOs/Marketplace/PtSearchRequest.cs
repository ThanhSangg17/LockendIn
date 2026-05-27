using System;

namespace LockedIn.BusinessObject.DTOs.Marketplace;

public class PtSearchRequest
{
    public string? Keyword { get; set; }
    public decimal? MinRating { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
