 using Microsoft.AspNetCore.Authentication;

namespace Templates.MinimalApi.Auth;

public class ApiKeyAuthSchemeOptions : AuthenticationSchemeOptions
{
  //TODO : setup system to pull keys from DB
  public string ApiKey { get; set; } = Environment.GetEnvironmentVariable("TransactionAPIKey");
}
