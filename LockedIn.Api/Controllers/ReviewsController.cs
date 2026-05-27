using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _service;

    public ReviewsController(IReviewService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReviewAsync()
    {
        var result = await _service.CreateReviewAsync();
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
    public async Task<IActionResult> UpdateReviewAsync(Guid reviewId)
    {
        var result = await _service.UpdateReviewAsync(reviewId);
        return Ok(result);
    }

    [HttpDelete("{reviewId}")]
    public async Task<IActionResult> DeleteReviewAsync(Guid reviewId)
    {
        var result = await _service.DeleteReviewAsync(reviewId);
        return Ok(result);
    }

}
