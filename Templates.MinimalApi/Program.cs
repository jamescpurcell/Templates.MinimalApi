using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Templates.MinimalApi.Auth;
using Templates.MinimalApi.Data;
using Templates.MinimalApi.Models;
using Templates.MinimalApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
  "appsettings.Local.json", true, true);

// Authentication
builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
  .AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
builder.Services.AddAuthorization();

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

app.UseAuthorization();

// Db initialation
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

// endpoints
app.MapPost("books",
  [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
  async (Book book, IBookService bookService,
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

app.MapGet("books",
  [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
  async (IBookService bookService, string? searchTerm) =>
{
  if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
  {
    var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
    return Results.Ok(matchedBooks);
  }
  var books = await bookService.GetAllAsync();
  return Results.Ok(books);
});

app.MapGet("books/{isbn}",
  [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
  async (string isbn, IBookService bookService) =>
{
  var book = await bookService.GetByIsbnAsync(isbn);
  return book is not null ? Results.Ok(book) : Results.BadRequest();
});

app.MapPut("books/{isbn}",
  [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
  async (string isbn, Book book, IBookService bookService,
  IValidator<Book> validator) =>
{
  book.Isbn = isbn;
  var validationResult = await validator.ValidateAsync(book);
  if (!validationResult.IsValid)
  {
    return Results.BadRequest(validationResult.Errors);
  }

  var updated = await bookService.UpdateAsync(book);
  return updated ? Results.Ok(book) : Results.NotFound();
});

app.MapDelete("books/{isbn}",
  [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
  async (string isbn, IBookService bookService) =>
{
  var deleted = await bookService.DeleteAsync(isbn);
  return deleted ? Results.NoContent() : Results.NotFound();
});

// application start
app.Run();
