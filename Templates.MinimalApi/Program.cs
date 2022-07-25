using FluentValidation;
using FluentValidation.Results;
using Templates.MinimalApi.Data;
using Templates.MinimalApi.Models;
using Templates.MinimalApi.Services;
using Templates.MinimalApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// Swashbuckle specific services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Db Setup
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
  new SQLServerConnectionFactory(
    builder.Configuration.GetValue<string>("Database:ConnectionString")));
builder.Services.AddSingleton<DatabaseInitializer>();

// Application Services
builder.Services.AddSingleton<IBookService, BookService>();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// let's build some middleware
var app = builder.Build();

// Swagger at https://localhost/swagger
app.UseSwagger(options => { });
app.UseSwaggerUI(options => { });

// Db initialation
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

// endpoints for now!
app.MapPost("books", async (Book book, IBookService bookService,
  IValidator<Book> validator) =>
{
  var validationResult = await validator.ValidateAsync(book);
  if (!validationResult.IsValid)
  {
    return Results.BadRequest(validationResult.Errors);
  }

  var created = await bookService.CreateAsync(book);
  if (!created)
  {
    return Results.BadRequest(new List<ValidationFailure>
    {
      new ("Isbn","A book with this ISBN-13 already exists")
    });
  }

  return Results.Created($"/books/{book.Isbn}", book);
});

app.MapGet("books", async (IBookService bookService, string? searchTerm) =>
{
  if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
  {
    var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
    return Results.Ok(matchedBooks);
  }
  var books = await bookService.GetAllAsync();
  return Results.Ok(books);
});

app.MapGet("books/{isbn}", async (string isbn, IBookService bookService) =>
{
  var book = await bookService.GetByIsbnAsync(isbn);
  return book is not null ? Results.Ok(book) : Results.BadRequest();
});

// application start
app.Run();
