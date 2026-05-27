using System;

namespace LockedIn.BusinessObject.DTOs.Reviews;

public class UpdateReviewRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
