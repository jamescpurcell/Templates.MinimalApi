using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Templates.MinimalApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<PeopleService>();
builder.Services.AddSingleton<GuidGenerator>();
var app = builder.Build();

// :int prevents string routes as just a 404 - not found 
app.MapGet("get-params/{age:int}",(int age) =>
{
  return $"Age provided was {age}";
});

// :regex prevents special chars in the route as a 404 - not found
app.MapGet("cars/{carId:regex(^[a-z0-9]+$)}", (string carId) =>
{
  return $"Car id provided was: {carId}";
});

// limits the length of the route object
app.MapGet("books/{isbn:length(13)}", (string isbn) =>
{
  return $"ISBN was: {isbn}";
});

// since route doesn't have searchTerm, it thinks it's a query string.
app.MapGet("people/search",
  (string? searchTerm, PeopleService peopleService) =>
{
  if (searchTerm is null)
  {
    Results.NotFound();
  }

  var results = peopleService.Search(searchTerm);
  return Results.Ok(results);
});

app.MapGet("mix/{routeParam}", (
  [FromRoute] string routeParam,
  [FromQuery(Name = "query")] int queryParam,
  [FromServices] GuidGenerator guidGenerator,
  [FromHeader(Name = "Accept-Encoding")] string encoding) =>
{
  return $"{routeParam} {queryParam} {guidGenerator} {encoding}";
});

app.MapPost("people", ([FromBody] Person person) =>
{
  return Results.Ok(person);
});

app.MapGet("httpcontext-1", async context =>
{
  await context.Response.WriteAsync("Hello from HttpContext 1");
});

app.MapGet("httpcontext-2", async (HttpContext context) =>
{
  await context.Response.WriteAsync("Hello from HttpContext 2");
});


app.MapGet("http", (HttpRequest request,HttpResponse response) =>
{
  return Results.Ok();
});

app.MapGet("claims", (ClaimsPrincipal user) =>
{
  
});

app.MapGet("cancel", (CancellationToken token) =>
{
  token.ThrowIfCancellationRequested();
  return Results.Ok();
});

app.Run();

/*public record Person(string FullName);

public class PeopleService
{
  private readonly List<Person> _people = new()
  {
    new Person("James Purcell"),
    new Person("Chloe Porter"),
    new Person("Lee Kennedy")
  };


  public IEnumerable<Person> Search(string searchTerm)
  {
    return _people.Where(x => x.FullName.Contains(
      searchTerm, StringComparison.OrdinalIgnoreCase));
  }
}*/
