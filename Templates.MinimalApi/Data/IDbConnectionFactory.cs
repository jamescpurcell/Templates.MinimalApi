using System.Data;

namespace Templates.MinimalApi.Data;

public interface IDbConnectionFactory
{
  Task<IDbConnection> CreateConnectionAsync();
}
