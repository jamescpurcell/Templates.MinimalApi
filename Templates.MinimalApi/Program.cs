using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Templates.MinimalApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<GuidGenerator>();
var app = builder.Build();

//location?latlong=1.65,0.10
app.MapGet("map-point",(MapPoint? point) =>
{
  return Results.Ok(point);
});

// or the body of a POST
app.MapPost("map-point",(MapPoint point) =>
{
  return Results.Ok(point);
});

app.Run();
