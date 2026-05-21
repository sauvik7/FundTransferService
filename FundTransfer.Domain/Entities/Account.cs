namespace FundTransfer.Domain.Entities;

public class Account
{
    public string AccountId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}
