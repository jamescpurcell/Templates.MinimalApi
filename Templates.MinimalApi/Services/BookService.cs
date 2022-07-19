using Dapper;
using Templates.MinimalApi.Data;
using Templates.MinimalApi.Models;

namespace Templates.MinimalApi.Services;

public class BookService : IBookService
{
  private readonly IDbConnectionFactory _connectionFactory;
  public BookService(IDbConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }

  public async Task<bool> CreateAsync(Book book)
  {
    // var existingBook = await GetByIsbnAsync(book.isbn);
    // if (existingBook is not null)
    //{
    //  return false;
    //}
    using var connection = await _connectionFactory.CreateConnectionAsync();
    var result = await connection.ExecuteAsync(
      @"INSERT INTO Books (Isbn, Title, Author, ShortDescription, PageCount, ReleaseDate)
      VALUES (@Isbn, @Title, @Author, @ShortDescription, @PageCount, @ReleaseDate)",
      book);
    return result > 0;
  }

  public Task<bool> DeleteAsync(string isbn)
  {
    throw new NotImplementedException();
  }

  public async Task<IEnumerable<Book>> GetAllAsync()
  {
    using var connection = await _connectionFactory.CreateConnectionAsync();
    return await connection.QueryAsync<Book>("SELECT * FROM Books");
  }

  public async Task<Book?> GetByIsbnAsync(string isbn)
  {
    using var connection = await _connectionFactory.CreateConnectionAsync();
    return connection.QuerySingleOrDefault<Book>(
      "SELECT * FROM Books WHERE Isbn = @Isbn", new { Isbn = isbn });
  }

  public Task<IEnumerable<Book>> SearchByTitleAsync(string searchTerm)
  {
    throw new NotImplementedException();
  }

  public Task<bool> UpdateAsync(Book book)
  {
    throw new NotImplementedException();
  }
}
