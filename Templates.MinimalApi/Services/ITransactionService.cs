using Templates.MinimalApi.Models;

namespace Templates.MinimalApi.Services;

public interface ITransactionService
{
  public Task<bool> CreateAsync(Transaction transaction);
  public Task<Transaction?> GetByIdAsync(long id);
  public Task<IEnumerable<Transaction>> GetAllAsync();
  public Task<IEnumerable<Transaction>> SearchByDescriptionAsync(string description);
  public Task<bool> UpdateAsync(Transaction transaction);
  public Task<bool> DeleteAsync(long id);
}
