var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("get-example", () => "Hello from get!");
app.MapPost("post-example", () => "Hello from post!");

app.Run();
