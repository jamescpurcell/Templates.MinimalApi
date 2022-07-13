using Templates.MinimalApi.Data;
using Templates.MinimalApi.Models;
using Templates.MinimalApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Swashbuckle specific services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Db Setup
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
  new SQLServerConnectionFactory(
    builder.Configuration.GetValue<string>("Database:ConnectionString")));
builder.Services.AddSingleton<DatabaseInitializer>();

// Repos
builder.Services.AddSingleton<IBookService, BookService>();

var app = builder.Build();

// Swagger at https://localhost/swagger
app.UseSwagger(options => { });
app.UseSwaggerUI(options => { });

// Db initialation
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.MapPost("books", async (Book book, IBookService bookService) =>
{
  var created = await bookService.CreateAsync(book);
  if (!created)
  {
    return Results.BadRequest(new
    {
      errorMessage = "A book with this ISBN-13 already exists"
    });
  }

  return Results.Created($"/books/{book.Isbn}", book);
});

app.Run();
