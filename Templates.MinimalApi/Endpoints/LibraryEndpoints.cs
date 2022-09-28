﻿using FluentValidation.Results;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Templates.MinimalApi.Auth;
using Templates.MinimalApi.Endpoints.Internal;
using Templates.MinimalApi.Models;
using Templates.MinimalApi.Services;

namespace Templates.MinimalApi.Endpoints;

public class LibraryEndpoints : IEndpoints
{
  public static void DefineEndpoints(IEndpointRouteBuilder app)
  {
    app.MapPost("books",
      [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
      async (Book book, IBookService bookService,
      IValidator<Book> validator, LinkGenerator linker,
      HttpContext httpContext) =>
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

      // nullable at the end
      var path = linker.GetPathByName("GetBook", new { isbn = book.Isbn })!;
      //var locationUri = linker.GetUriByName(httpContext, "GetBook", new { isbn = book.Isbn })!;
      return Results.Created(path, book);
      // one way
      //return Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book);
      //another way that could be a bad hard code
      //return Results.Created($"/books/{book.Isbn}", book);
    }).WithName("CreateBook")
    .Accepts<Book>("application/json")
    .Produces<Book>(201)
    .Produces<IEnumerable<ValidationFailure>>(400)
    .WithTags("Books");

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
    }).WithName("GetBooks")
      .Produces<IEnumerable<Book>>(200)
      .WithTags("Books");

    app.MapGet("books/{isbn}",
      [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
      async (string isbn, IBookService bookService) =>
    {
      var book = await bookService.GetByIsbnAsync(isbn);
      return book is not null ? Results.Ok(book) : Results.BadRequest();
    }).WithName("GetBook")
    .Produces<Book>(200)
    .WithTags("Books");

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
      }).WithName("UpdateBook")
      .Accepts<Book>("application/json")
      .Produces<Book>(200)
      .Produces<IEnumerable<ValidationFailure>>(400)
      .WithTags("Books");

    app.MapDelete("books/{isbn}",
      [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
      async (string isbn, IBookService bookService) =>
    {
      var deleted = await bookService.DeleteAsync(isbn);
      return deleted ? Results.NoContent() : Results.NotFound();
    }).WithName("DeleteBook")
    .Produces(204)
    .Produces(404)
    .WithTags("Books");

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
  public static void AddServices(IServiceCollection services,
    IConfiguration configuration)
  {
    services.AddSingleton<IBookService, BookService>();
  }
}
