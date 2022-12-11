using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Runtime.InteropServices;
using Templates.MinimalApi;
using Templates.MinimalApi.Models;

namespace Library.Api.Tests.Integration;

public class TransactionEndpointsTests
  : IClassFixture<TransactionApiFactory>, IAsyncLifetime
{
  private readonly TransactionApiFactory _factory;
  private readonly List<long> _createdIds = new();

  public TransactionEndpointsTests(TransactionApiFactory factory)
  {
    _factory = factory;
  }

  [Fact]
  public async Task CreateTransaction_CreatesTransaction_WhenIsCorrect()
  {
    // Arrange
    var httpClient = _factory.CreateClient();
    var transaction = GenerateTransaction();

    // Act
    var result = await httpClient.PostAsJsonAsync("/transactions", transaction);
    _createdIds.Add(transaction.Id); // cleanup
    var createdTransaction = await result.Content.ReadFromJsonAsync<Transaction>();

    // Assert
    // 201 - Created
    result.StatusCode.Should().Be(HttpStatusCode.Created);
    // Created object matches sent object
    createdTransaction.Should().BeEquivalentTo(transaction);
    // Proper Path in Header
    result.Headers.Location.Should().Be($"/transactions/{transaction.Id}");
  }

  [Fact]
  public async Task CreateTransaction_Fails_WhenIdIsInvalid()
  {
    // Arrange
    var httpClient = _factory.CreateClient();
    var transaction = GenerateTransaction();
    transaction.Id = -6;

    // Act
    var result = await httpClient.PostAsJsonAsync("/transactions", transaction);
    _createdIds.Add(transaction.Id); // cleanup
    var errors = await result.Content.ReadFromJsonAsync<
      IEnumerable<ValidationError>>();
    var error = errors!.Single();

    // Assert
    result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    error.PropertyName.Should().Be("Id");
    error.ErrorMessage.Should().Be("Value was not a valid Id");
  }

  [Fact]
  public async Task CreateTransaction_Fails_TransactionExists()
  {
    // Arrange
    var httpClient = _factory.CreateClient();
    var transaction = GenerateTransaction();

    // Act
    // Create the Book First
    await httpClient.PostAsJsonAsync("/transactions", transaction);
    _createdIds.Add(transaction.Id); // cleanup
    // Then try to recreate the same book
    var result = await httpClient.PostAsJsonAsync("/transactions", transaction);
    var errors = await result.Content.ReadFromJsonAsync<
      IEnumerable<ValidationError>>();
    var error = errors!.Single();

    // Assert
    result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    error.PropertyName.Should().Be("Id");
    error.ErrorMessage.Should().Be("A transaction with this Id already exists");
  }

  [Fact]
  public async Task GetTransaction_ReturnsTransaction_WhenTransactionExists()
  {
    // Arrange
    var httpClient = _factory.CreateClient();
    var transaction = GenerateTransaction();
    await httpClient.PostAsJsonAsync("/transactions", transaction);
    _createdIds.Add(transaction.Id);

    // Act
    var result = await httpClient.GetAsync($"/transactions/{transaction.Id}");
    var existingTransaction = await result.Content.ReadFromJsonAsync<Transaction>();

    // Assert
    existingTransaction.Should().BeEquivalentTo(transaction);
    result.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task GetTransaction_ReturnsNotFound_WhenTransactionDoesNotExist()
  {
    // Arrange
    var httpClient = _factory.CreateClient();
    var id = GenerateId();

    // Act
    var result = await httpClient.GetAsync($"/transactions/{id}");

    // Assert
    result.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  // Some sort of cache issue with SQLite
  [Fact] 
  public async Task GetAllTransactions_ReturnsAllTransactions_WhenTransactionsExist()
  {
    // Arrange
    var httpClient = _factory.CreateClient();
    var transaction = GenerateTransaction();
    await httpClient.PostAsJsonAsync("/transactions", transaction);
    _createdIds.Add(transaction.Id);
    var transactions = new List<Transaction> { transaction };

    // Act
    var result = await httpClient.GetAsync("/transactions");
    var returnedTransactions = await result.Content.ReadFromJsonAsync<List<Transaction>>();

    // Assert
    result.StatusCode.Should().Be(HttpStatusCode.OK);
    returnedTransactions.Should().BeEquivalentTo(transactions);
  }

  [Fact]
  public async Task GetAllTransactions_ReturnsNoTransactions_WhenNoTransactionsExist()
  {
    // Arrange
    var httpClient = _factory.CreateClient();

    // Act
    var result = await httpClient.GetAsync("/transactions");
    var returnedTransactions = await result.Content.ReadFromJsonAsync<List<Transaction>>();

    // Assert
    result.StatusCode.Should().Be(HttpStatusCode.OK);
    returnedTransactions.Should().BeEmpty();
  }

  [Fact]
  public async Task SearchTransactions_ReturnsTransactions_WhenDescriptionMatches()
  {
    // Arrange
    var httpClient = _factory.CreateClient();
    var transaction = GenerateTransaction();
    transaction.Description = "test";
    await httpClient.PostAsJsonAsync("/transactions", transaction);
    _createdIds.Add(transaction.Id);
    var transactions = new List<Transaction> { transaction };

    // Act
    var result = await httpClient.GetAsync("/transactions?Description=test");
    var returnedTransactions = await result.Content.ReadFromJsonAsync<List<Transaction>>();

    // Assert
    result.StatusCode.Should().Be(HttpStatusCode.OK);
    returnedTransactions.Should().BeEquivalentTo(transactions);
  }

  [Fact]
  public async Task UpdateTransaction_UpdatesTransaction_WhenDataIsCorrect()
  {
    // Arrange
    var httpClient = _factory.CreateClient();
    var transaction = GenerateTransaction();
    await httpClient.PostAsJsonAsync("/transactions", transaction);
    _createdIds.Add(transaction.Id);

    // Act
    transaction.Description = "test for update";
    var result = await httpClient.PutAsJsonAsync($"/transactions/{transaction.Id}", transaction);
    var updatedTransaction = await result.Content.ReadFromJsonAsync<Transaction>();

    // Assert
    result.StatusCode.Should().Be(HttpStatusCode.OK);
    updatedTransaction.Should().BeEquivalentTo(transaction);
  }

  [Fact]
  public async Task UpdateTransaction_DoesNotUpdateTransaction_WhenDataIsIncorrect()
  {
    // Arrange
    var httpClient = _factory.CreateClient();
    var transaction = GenerateTransaction();

    // Act
    transaction.Amount = 200.00M;
    var result = await httpClient.PutAsJsonAsync($"/transactions/{transaction.Id}", transaction);
    var errors = await result.Content.ReadFromJsonAsync<
      IEnumerable<ValidationError>>();
    var error = errors!.Single();

    // Assert
    result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    error.PropertyName.Should().Be("Amount");
    error.ErrorMessage.Should().Be("'Amount' must not be empty.");
  }

  [Fact]
  public async Task UpdateTransaction_ReturnsNotFound_WhenTransactionDoesNotExist()
  {
    // Arrange
    var httpClient = _factory.CreateClient();
    var transaction = GenerateTransaction();

    // Act
    var result = await httpClient.PutAsJsonAsync($"/transactions/{transaction.Id}", transaction);

    // Assert
    result.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task DeleteTransaction_ReturnsNotFound_WhenTransactionDoesNotExist()
  {
    // Arrange
    var httpClient = _factory.CreateClient();
    var id = GenerateId();

    // Act
    var result = await httpClient.DeleteAsync($"/transactions/{id}");

    // Assert
    result.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task DeleteTransaction_ReturnsNoContent_WhenTransactionDoesNotExist()
  {
    // Arrange
    var httpClient = _factory.CreateClient();
    var transaction = GenerateTransaction();
    await httpClient.PostAsJsonAsync("/transactions", transaction);
    _createdIds.Add(transaction.Id);

    // Act
    var result = await httpClient.DeleteAsync($"/transactions/{transaction.Id}");

    // Assert
    result.StatusCode.Should().Be(HttpStatusCode.NoContent);
  }

  private Transaction GenerateTransaction(string description = "test")
  {
    return new Transaction
    {
      Id = GenerateId(),
      Amount = 200.00M,
      Description = description,
      Type = "credit",
      IpAddressV4 = "10.123.123.123",
      IpAddressV6 = "ln80::9f22:8e6f:6009:b42b%9"
    };
  }

  private long GenerateId()
  {
    return Random.Shared.NextInt64(2, 1000);
  }

  public Task InitializeAsync() => Task.CompletedTask;

  public async Task DisposeAsync()
  {
    var httpClient = _factory.CreateClient();

    foreach (var createdIds in _createdIds)
    {
      await httpClient.DeleteAsync($"/transactions/{createdIds}");
    }
  }
}
