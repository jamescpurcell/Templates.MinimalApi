﻿using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Templates.MinimalApi.Models;
using System.Net;
using Templates.MinimalApi;

namespace Library.Api.Tests.Integration;

public class LibraryEndpointsTests
  : IClassFixture<WebApplicationFactory<IApiMarker>>
{
  private readonly WebApplicationFactory<IApiMarker> _factory;

  public LibraryEndpointsTests(WebApplicationFactory<IApiMarker> factory)
  {
    _factory = factory;
  }

  [Fact]
  public async Task CreateBook_CreatesBook_WhenIsCorrect()
  {
    // Arrange
    var httpClient = _factory.CreateClient();
    var book = GenerateBook();

    // Act
    var result = await httpClient.PostAsJsonAsync("/books", book);
    var createdBook = await result.Content.ReadFromJsonAsync<Book>();

    // Assert
    // 201 - Created
    result.StatusCode.Should().Be(HttpStatusCode.Created);
    // Created object matches sent object
    createdBook.Should().BeEquivalentTo(book);
    // Proper Path in Header
    result.Headers.Location.Should().Be($"/Books/{book.Isbn}");
  }

  [Fact]
  public async Task CreateBook_Fails_WhenIsbnIsInvalid()
  {
    // Arrange
    var httpClient = _factory.CreateClient();
    var book = GenerateBook();
    book.Isbn = "INVALID";

    // Act
    var result = await httpClient.PostAsJsonAsync("/books", book);
    var errors = await result.Content.ReadFromJsonAsync<
      IEnumerable<ValidationError>>();
    var error = errors!.Single();

    // Assert
    result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    error.PropertyName.Should().Be("Isbn");
    error.ErrorMessage.Should().Be("Value was not a valid ISBN-13");
  }

  [Fact]
  public async Task CreateBook_Fails_BookExists()
  {
    // Arrange
    var httpClient = _factory.CreateClient();
    var book = GenerateBook();

    // Act
    // Create the Book First
    await httpClient.PostAsJsonAsync("/books", book);
    // Then try to recreate the same book
    var result = await httpClient.PostAsJsonAsync("/books", book);

    var errors = await result.Content.ReadFromJsonAsync<
      IEnumerable<ValidationError>>();
    var error = errors!.Single();

    // Assert
    result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    error.PropertyName.Should().Be("Isbn");
    error.ErrorMessage.Should().Be("A book with this ISBN-13 already exists");
  }

  private Book GenerateBook(string title = "The Dirty Coder")
  {
    return new Book
    {
      Isbn = GenerateIsbn(),
      Title = title,
      Author = "James Purcell",
      PageCount = 600,
      ShortDescription = "All my tricks in one book",
      ReleaseDate = new DateTime(2023, 1, 1)
    };
  }
  private string GenerateIsbn()
  {
    return $"{Random.Shared.Next(100, 999)}-" +
           $"{Random.Shared.Next(1000000000, 2100999999)}"; 
  }
}