using Templates.MinimalApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Swashbuckle specific services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Db Setup
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
  new SQLServerConnectionFactory(
    builder.Configuration.GetValue<string>("Database:ConnectionString")));
builder.Services.AddSingleton<DatabaseInitializer>();

var app = builder.Build();

// Swagger at https://localhost/swagger
app.UseSwagger(options => { });
app.UseSwaggerUI(options => { });

// Db initialation
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.MapGet("Logging", (ILogger<Program> logger) =>
{
  logger.LogInformation("Hello from info stream");
  return Results.Ok();
});

app.Run();
