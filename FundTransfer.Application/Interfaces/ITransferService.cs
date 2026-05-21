using FundTransfer.Application.DTOs;

namespace FundTransfer.Application.Interfaces;

public interface ITransferService
{
    Task<(bool Success, string? Error)> ProcessAsync(TransferRequest request);
}