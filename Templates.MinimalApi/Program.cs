using Templates.MinimalApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<GuidGenerator>();

// Swashbuckle specific services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// middleware setup for swagger at https://localhost/swagger
app.UseSwagger(options => { });
app.UseSwaggerUI(options => { });

app.MapGet("Logging", (ILogger<Program> logger) =>
{
  logger.LogInformation("Hello from info stream");
  return Results.Ok();
});

app.Run();
