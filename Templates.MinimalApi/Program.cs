using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Templates.MinimalApi.Auth;
using Templates.MinimalApi.Data;
using Templates.MinimalApi.Endpoints.Internal;

Log.Logger = new LoggerConfiguration()
  .WriteTo.Console()
  .CreateBootstrapLogger();

Log.Information("Starting Templates.MinimalApi API Application...");

try
{
  // Host Builder / Environment Setup
  var builder = WebApplication.CreateBuilder(new WebApplicationOptions
  {
    Args = args,
    //WebRootPath = "./wwwroot",
    //EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    //ApplicationName = "Library.Api"
  });
  var environment = builder.Environment.EnvironmentName;
  bool isDevEnv = environment.Any(
    env => env.Equals("Development") || env.Equals("Local"));

  builder.Host.UseSerilog((context, config) => config
    .WriteTo.Console()
    .ReadFrom.Configuration(context.Configuration));

  builder.Services.AddCors(options =>
  {
    options.AddPolicy("AnyOrigin", x => x.AllowAnyOrigin());
  });

  builder.Services.Configure<JsonOptions>(options =>
  {
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.IncludeFields = true;
  });

  // Authentication - Basic Auth
  builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
    .AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
  builder.Services.AddAuthorization();

  // environment based config. might not be needed
  //builder.Configuration.AddJsonFile(
  //  $"appsettings.{builder.Environment.EnvironmentName}
  //  .json", true, true);

  // Swashbuckle specific services
  builder.Services.AddEndpointsApiExplorer();
  builder.Services.AddSwaggerGen();

  // DB Setup
  var connectionString = new ConnectionStringBuilder(environment).Build();
  // Log.Debug($"Connection String: {connectionString}");
  builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SQLServerConnectionFactory(connectionString));
  // builder.Services.AddSingleton<DatabaseInitializer>();

  // Controller Endpoints
  builder.Services.AddEndpoints<Program>(builder.Configuration);

  // Validators
  builder.Services.AddValidatorsFromAssemblyContaining<Program>();

  // HealthChecks
  builder.Services.AddHealthChecks();

  // let's build some middleware
  var app = builder.Build();

  if (isDevEnv) { }

  app.UseCors();

  // Swagger at https://localhost/swagger
  app.UseSwagger(options => { });
  app.UseSwaggerUI(options => { });

  app.UseAuthorization();

  // Health Checks
  app.UseHealthChecks("/health/ready", new HealthCheckOptions()
  {
    AllowCachingResponses = false
  });
  app.UseHealthChecks("/health/live", new HealthCheckOptions()
  {
    AllowCachingResponses = false
  });

  app.UseSerilogRequestLogging();

  app.UseHttpsRedirection();

  // endpoints
  app.UseEndpoints<Program>();

  app.Run();

}
catch (Exception ex)
{
  Log.Fatal(ex, $"Unhandled Exception at Startup: {ex}");
}
finally
{
  Log.Information("Application Shutdown Complete");
  Log.CloseAndFlush();
}
