using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.DTOs.Feedbacks;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/feedbacks")]
[Authorize]
public class FeedbacksController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;

    public FeedbacksController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateFeedbackAsync([FromBody] CreateFeedbackRequest request)
    {
        var result = await _feedbackService.CreateFeedbackAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFeedbacksAsync()
    {
        var result = await _feedbackService.GetAllFeedbacksAsync();
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyFeedbacksAsync()
    {
        var result = await _feedbackService.GetMyFeedbacksAsync();
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFeedbackAsync(Guid id, [FromBody] UpdateFeedbackRequest request)
    {
        var result = await _feedbackService.UpdateFeedbackAsync(id, request);
        if (!result.Success)
        {
            if (result.Message != null && result.Message.Contains("chính mình"))
            {
                return StatusCode(403, result);
            }
            if (result.Message != null && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFeedbackAsync(Guid id)
    {
        var result = await _feedbackService.DeleteMyFeedbackAsync(id);
        if (!result.Success)
        {
            if (result.Message != null && result.Message.Contains("chính mình"))
            {
                return StatusCode(403, result);
            }
            if (result.Message != null && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }
        return Ok(result);
    }
}
