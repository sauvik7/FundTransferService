using FundTransfer.Domain.Entities;

namespace FundTransfer.Application.Interfaces;

public interface IAccountStore
{
    Task<Account?> GetAsync(string accountId);
    Task AddAsync(Account account);
    Task SaveChangesAsync();
}