var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("get-example", () => "Hello from get!");
app.MapPost("post-example", () => "Hello from post!");

app.MapGet("ok-object", () => Results.Ok(new
{
  Name = "James Purcell"
}));

app.MapGet("slow-request", async () =>
{
  await Task.Delay(1000);
  return Results.Ok(new
  {
    Name = "James Purcell"
  });
});

app.Run();
