using FundTransfer.Application.DTOs;
using FundTransfer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FundTransfer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransferController(ITransferService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        // ✅ Model validation
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, error) = await service.ProcessAsync(request);

        // ✅ Business failure
        if (!success)
        {
            return BadRequest(new
            {
                error = error
            });
        }

        // ✅ Success
        return Ok(new
        {
            message = "Transfer successful",
            requestId = request.RequestId
        });
    }
}