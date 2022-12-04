using Microsoft.Data.SqlClient;
using System.Data;

namespace Templates.MinimalApi.Data;

public class SQLServerConnectionFactory : IDbConnectionFactory
{
  private readonly string _connectionString;
  public SQLServerConnectionFactory(string connectionString)
  {
    _connectionString = connectionString;
  }

  public async Task<IDbConnection> CreateConnectionAsync()
  {
    var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();
    return connection;
  }
}
