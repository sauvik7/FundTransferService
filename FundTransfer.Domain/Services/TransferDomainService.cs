using FundTransfer.Domain.Entities;

namespace FundTransfer.Domain.Services;

public class TransferDomainService
{
    public Transaction Execute(
        Account from,
        Account to,
        string requestId,
        decimal amount)
    {
        var tx = new Transaction(requestId, from.AccountId, to.AccountId, amount);

        from.Debit(amount);
        to.Credit(amount);

        tx.MarkSuccess();
        return tx;
    }
}