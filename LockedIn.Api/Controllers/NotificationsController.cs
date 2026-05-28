using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service)
    {
        _service = service;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyNotificationsAsync()
    {
        var result = await _service.GetMyNotificationsAsync();
        return Ok(result);
    }

    [HttpPatch("{notificationId}/read")]
    public async Task<IActionResult> MarkAsReadAsync(Guid notificationId)
    {
        var result = await _service.MarkAsReadAsync(notificationId);
        return Ok(result);
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsReadAsync()
    {
        var result = await _service.MarkAllAsReadAsync();
        return Ok(result);
    }

    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotificationAsync(Guid notificationId)
    {
        var result = await _service.DeleteNotificationAsync(notificationId);
        return Ok(result);
    }

}
