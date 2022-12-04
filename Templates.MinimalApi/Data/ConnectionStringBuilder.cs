using Microsoft.Data.SqlClient;

namespace Templates.MinimalApi.Data;

public class ConnectionStringBuilder
{
  private readonly string _environment;
  public ConnectionStringBuilder(string environment)
  {
    _environment = environment;
  }

  public string Build()
  {
    var connectionString = new SqlConnectionStringBuilder();
    connectionString.DataSource =
      Environment.GetEnvironmentVariable(
        "SQL_DATASOURCE") ?? string.Empty;
    connectionString.InitialCatalog =
      Environment.GetEnvironmentVariable(
        "SQL_INITIALCATALOG") ?? string.Empty;
    connectionString.UserID =
      Environment.GetEnvironmentVariable(
        "SQL_USERID") ?? string.Empty;
    connectionString.Password =
      Environment.GetEnvironmentVariable(
        "SQL_PASSWORD") ?? string.Empty;

    if (_environment == "Local")
    {
      connectionString.IntegratedSecurity = true;
    }
    else
    {
      connectionString.IntegratedSecurity = false;
    }

    //Log.Debug($"ConnectionString: {connectionString}");

    return connectionString.ToString();
  }
}
