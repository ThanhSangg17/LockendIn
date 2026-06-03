using System;
using System.ComponentModel.DataAnnotations;

namespace LockedIn.BusinessObject.DTOs.Marketplace;

public class PtSearchRequest
{
    [StringLength(100, ErrorMessage = "Keyword cannot exceed 100 characters.")]
    public string? Keyword { get; set; }

    [Range(0, 5, ErrorMessage = "MinRating must be between 0 and 5.")]
    public decimal? MinRating { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1.")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "Page size must be between 1 and 50.")]
    public int PageSize { get; set; } = 10;
}
