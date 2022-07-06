using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
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

app.MapGet("json-raw-obj", () => new { message = "Hello" });
app.MapGet("ok-obj", () => Results.Ok(new { message = "Hello" }));
app.MapGet("json-obj", () => Results.Json(new { message = "Hello" }));
app.MapGet("text-string", () => Results.Text("Hello"));

app.MapGet("stream-result", () =>
{
  var memoryStream = new MemoryStream();
  var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
  streamWriter.Write("Hello");
  streamWriter.Flush();
  memoryStream.Seek(0, SeekOrigin.Begin);
  return Results.Stream(memoryStream);
});

app.MapGet("redirect", () => Results.Redirect("https://bing.com"));

app.MapGet("download", () => Results.File("C:\\Temp\\Test.txt"));

app.Run();
