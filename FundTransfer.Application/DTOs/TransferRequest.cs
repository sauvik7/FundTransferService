namespace FundTransfer.Application.DTOs;

public class TransferRequest
{
    public string FromAccount { get; set; } = string.Empty;
    public string ToAccount { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}