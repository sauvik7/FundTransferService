using Microsoft.AspNetCore.Mvc;
using FundTransfer.Application.DTOs;
using FundTransfer.Application.Services;

namespace FundTransfer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferController(TransferService transferService, ILogger<TransferController> logger) : ControllerBase
    {
        private readonly TransferService _transferService = transferService;
        private readonly ILogger<TransferController> _logger = logger;

        [HttpPost]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
        {
            _logger.LogInformation("Processing transfer: {RequestId}", request.RequestId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (Success, Error) = await _transferService.ProcessAsync(request);

            if (!Success)
            {
                _logger.LogWarning("Transfer failed: {Error}", Error);
                return BadRequest(new { error = Error });
            }

            return Ok(new { message = "Transfer successful" });
        }
    }
}