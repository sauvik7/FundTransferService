using FundTransfer.Application.DTOs;
using FundTransfer.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FundTransfer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransferController(TransferService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (success, error) = await service.ProcessAsync(request);

        if (!success)
        {
            return Problem(
                detail: error,
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        return Ok(new
        {
            message = "Transfer successful",
            requestId = request.RequestId
        });
    }
}