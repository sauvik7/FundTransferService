using FundTransfer.Application.DTOs;
using FundTransfer.Application.Interfaces;

namespace FundTransfer.Infrastructure;

public class SimpleThresholdFraudService : IFraudService
{
    private readonly decimal _threshold;

    public SimpleThresholdFraudService(decimal threshold = 100000m)
    {
        _threshold = threshold;
    }

    public bool IsFraudulent(TransferRequest request, out string? reason)
    {
        if (request.Amount > _threshold)
        {
            reason = "Transaction limit exceeded";
            return true;
        }

        reason = null;
        return false;
    }
}
