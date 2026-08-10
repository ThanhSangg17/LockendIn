using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.Transactions;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _service;

    public TransactionsController(ITransactionService service)
    {
        _service = service;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTransactionsAsync([FromQuery] TransactionSearchRequest request)
    {
        var result = await _service.GetMyTransactionsAsync(request);
        if (!result.Success)
        {
            if (result.Message != null && result.Message.Contains("not authenticated"))
                return Unauthorized(result);
            if (result.Message != null && result.Message.Contains("Access denied"))
                return StatusCode(403, result);
            return BadRequest(result);
        }
        return Ok(result);
    }
}
