using FluentValidation.Results;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Templates.MinimalApi.Auth;
using Templates.MinimalApi.Endpoints.Internal;
using Templates.MinimalApi.Models;
using Templates.MinimalApi.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Primitives;

namespace Templates.MinimalApi.Endpoints;

public class LibraryEndpoints : IEndpoints
{
  private const string ContentType = "application/json";
  private const string Tag = "Books";
  private const string BaseRoute = "Books";
  public static void DefineEndpoints(IEndpointRouteBuilder app)
  {
    app.MapPost(BaseRoute, CreateBookAsync)
      .WithName("CreateBook")
      .Accepts<Book>(ContentType)
      .Produces<Book>(201)
      .Produces<IEnumerable<ValidationFailure>>(400)
      .WithTags(Tag);

    app.MapGet(BaseRoute, GetAllBooksAsync)
      .WithName("GetBooks")
      .Produces<IEnumerable<Book>>(200)
      .WithTags(Tag);

    app.MapGet($"{BaseRoute}/{{isbn}}", GetBookByIsbn)
      .WithName("GetBook")
      .Produces<Book>(200)
      .WithTags(Tag);

    app.MapPut($"{BaseRoute}/{{isbn}}", UpdateBookAsync)
      .WithName("UpdateBook")
      .Accepts<Book>(ContentType)
      .Produces<Book>(200)
      .Produces<IEnumerable<ValidationFailure>>(400)
      .WithTags(Tag);

    app.MapDelete($"{BaseRoute}/{{isbn}}", DeleteBookAsync)
      .WithName("DeleteBook")
      .Produces(204)
      .Produces(404)
      .WithTags(Tag);

    // writing own extensions, ex. html
    app.MapGet("status", [EnableCors("AnyOrigin")] () =>
    {
      return Results.Extensions.Html(@"<!doctype html>
        <html>
          <head><title>Status Page</title></head>
          <body>
            <h1>Status Page</h1>
          </body>
        <html>");
    });
    //.RequireCors("AnyOrigin");// added with attribute above
    //.ExcludeFromDescription();// perfect to remove from swagger
  }

  internal static async Task<IResult> CreateBookAsync(
    Book book, IBookService bookService,
    IValidator<Book> validator)
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

    return Results.Created($"/{BaseRoute}/{book.Isbn}", book);
  }

  internal static async Task<IResult> GetAllBooksAsync(
    IBookService bookService, string? searchTerm)
  {
    if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
    {
      var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
      return Results.Ok(matchedBooks);
    }
    var books = await bookService.GetAllAsync();
    return Results.Ok(books);
  }

  internal static async Task<IResult> GetBookByIsbn(
    string isbn, IBookService bookService)
  {
    var book = await bookService.GetByIsbnAsync(isbn);
    return book is not null ? Results.Ok(book) : Results.BadRequest();
  }

  internal static async Task<IResult> UpdateBookAsync(
    string isbn, Book book,
    IBookService bookService,IValidator<Book> validator)
  {
    book.Isbn = isbn;
    var validationResult = await validator.ValidateAsync(book);
    if (!validationResult.IsValid)
    {
      return Results.BadRequest(validationResult.Errors);
    }

    var updated = await bookService.UpdateAsync(book);
    return updated ? Results.Ok(book) : Results.NotFound();
  }

  internal static async Task<IResult> DeleteBookAsync(
    string isbn, IBookService bookService)
  {
    var deleted = await bookService.DeleteAsync(isbn);
    return deleted ? Results.NoContent() : Results.NotFound();
  }

  public static void AddServices(IServiceCollection services,
    IConfiguration configuration)
  {
    services.AddSingleton<IBookService, BookService>();
  }
}
