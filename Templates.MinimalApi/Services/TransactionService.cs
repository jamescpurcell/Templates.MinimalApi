using Dapper;
using Templates.MinimalApi.Data;
using Templates.MinimalApi.Models;

namespace Templates.MinimalApi.Services;

public class TransactionService : ITransactionService
{
  private readonly IDbConnectionFactory _connectionFactory;
  public TransactionService(IDbConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }

  public async Task<bool> CreateAsync(Transaction transaction)
  {
    var existingTransaction = await GetByIdAsync(transaction.Id);
    if (existingTransaction is not null)
    {
      return false;
    }
    using var connection = await _connectionFactory.CreateConnectionAsync();
    var result = await connection.ExecuteAsync(
      @"INSERT INTO [dbo].[Transactions] (Id, Amount, Description, Type, IpAddressV4, IpAddressV6)
      VALUES (@Id, @Amount, @Description, @Type, @IpAddressV4, @IpAddressV6)",
      transaction);
    return result > 0;
  }

  public async Task<bool> DeleteAsync(long id)
  {
    using var connection = await _connectionFactory.CreateConnectionAsync();
    var result = await connection.ExecuteAsync(
      @"DELETE FROM [dbo].[Transactions] WHERE Id = @Id", new { Id = id});
    return result > 0;
  }

  public async Task<IEnumerable<Transaction>> GetAllAsync()
  {
    using var connection = await _connectionFactory.CreateConnectionAsync();
    return await connection.QueryAsync<Transaction>("SELECT * FROM [dbo].[Transactions]");
  }

  public async Task<Transaction?> GetByIdAsync(long id)
  {
    using var connection = await _connectionFactory.CreateConnectionAsync();
    return connection.QuerySingleOrDefault<Transaction>(
      "SELECT * FROM [dbo].[Transactions] WHERE Isbn = @Isbn", new { Id = id });
  }

  public async Task<IEnumerable<Transaction>> SearchByDescriptionAsync(string description)
  {
    // TODO - add null prevention
    using var connection = await _connectionFactory.CreateConnectionAsync();
    return await connection.QueryAsync<Transaction>(
      "SELECT * FROM [dbo].[Transactions] WHERE Description LIKE '%' || @Description ||'%'",
      new { Description = @description });
  }

  public async Task<bool> UpdateAsync(Transaction transaction)
  {
    var existingTransaction = await GetByIdAsync(transaction.Id);
    if (existingTransaction is null)
    {
      return false;
    }

    using var connection = await _connectionFactory.CreateConnectionAsync();
    var result = await connection.ExecuteAsync(
      @"UPDATE [dbo].[Transactions] SET
          Amount = @Amount,
          Description = @Description,
          Type = @Type,
          IpAddressV4 = @IpAddressV4,
          IpAddressV6 = @IpAddressV6
        WHERE Id = @Id", transaction);

    return result > 0;
  }
}
