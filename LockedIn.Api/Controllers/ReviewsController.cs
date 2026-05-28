using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Reviews;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _service;

    public ReviewsController(IReviewService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReviewAsync([FromBody] CreateReviewRequest request)
    {
        var result = await _service.CreateReviewAsync(request);
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyReviewsAsync()
    {
        var result = await _service.GetMyReviewsAsync();
        return Ok(result);
    }

    [HttpGet("pt/{ptProfileId}")]
    public async Task<IActionResult> GetReviewsByPtAsync(Guid ptProfileId)
    {
        var result = await _service.GetReviewsByPtAsync(ptProfileId);
        return Ok(result);
    }

    [HttpPut("{reviewId}")]
    public async Task<IActionResult> UpdateReviewAsync(Guid reviewId, [FromBody] UpdateReviewRequest request)
    {
        var result = await _service.UpdateReviewAsync(reviewId, request);
        return Ok(result);
    }

    [HttpDelete("{reviewId}")]
    public async Task<IActionResult> DeleteReviewAsync(Guid reviewId)
    {
        var result = await _service.DeleteReviewAsync(reviewId);
        return Ok(result);
    }
}
