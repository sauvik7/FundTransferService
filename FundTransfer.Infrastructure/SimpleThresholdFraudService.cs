using FundTransfer.Application.DTOs;
using FundTransfer.Application.Interfaces;

namespace FundTransfer.Infrastructure;

public class SimpleThresholdFraudService(decimal threshold = 100000m) : IFraudService
{
    public bool IsFraudulent(TransferRequest request, out string? reason)
    {
        if (request.Amount > threshold)
        {
            reason = "Transaction limit exceeded";
            return true;
        }

        reason = null;
        return false;
    }
}
