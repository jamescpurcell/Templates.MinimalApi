using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Templates.MinimalApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<GuidGenerator>();
var app = builder.Build();

app.MapGet("Logging", (ILogger<Program> logger) =>
{
  logger.LogInformation("Hello from info stream");
  return Results.Ok();
});

app.Run();
