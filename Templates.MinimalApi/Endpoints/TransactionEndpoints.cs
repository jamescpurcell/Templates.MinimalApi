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
using System.Transactions;
using Transaction = Templates.MinimalApi.Models.Transaction;

namespace Templates.MinimalApi.Endpoints;

public class TransactionEndpoints : IEndpoints
{
  private const string ContentType = "application/json";
  private const string Tag = "Transactions";
  private const string BaseRoute = "Transactions";
  public static void DefineEndpoints(IEndpointRouteBuilder app)
  {
    app.MapPost(BaseRoute, CreateTransactionAsync)
      .WithName("CreateTransaction")
      .Accepts<Transaction>(ContentType)
      .Produces<Transaction>(201)
      .Produces<IEnumerable<ValidationFailure>>(400)
      .WithTags(Tag);

    app.MapGet(BaseRoute, GetAllTransactionsAsync)
      .WithName("GetTransactions")
      .Produces<IEnumerable<Transaction>>(200)
      .WithTags(Tag);

    app.MapGet($"{BaseRoute}/{{id}}", GetTransactionById)
      .WithName("GetTransaction")
      .Produces<Transaction>(200)
      .WithTags(Tag);

    app.MapPut($"{BaseRoute}/{{id}}", UpdateTransactionAsync)
      .WithName("UpdateTransaction")
      .Accepts<Transaction>(ContentType)
      .Produces<Transaction>(200)
      .Produces<IEnumerable<ValidationFailure>>(400)
      .WithTags(Tag);

    app.MapDelete($"{BaseRoute}/{{id}}", DeleteTransactionAsync)
      .WithName("DeleteTransaction")
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

  internal static async Task<IResult> CreateTransactionAsync(
    Transaction transaction, ITransactionService transactionService,
    IValidator<Transaction> validator)
  {
    var validationResult = await validator.ValidateAsync(transaction);
    if (!validationResult.IsValid)
    {
      return Results.BadRequest(validationResult.Errors);
    }

    var created = await transactionService.CreateAsync(transaction);
    if (!created)
    {
      return Results.BadRequest(new List<ValidationFailure>
      {
        new ("Id","A transaction with that id already exists")
      });
    }

    return Results.Created($"/{BaseRoute}/{transaction.Id}", transaction);
  }

  internal static async Task<IResult> GetAllTransactionsAsync(
    ITransactionService transactionService, string? description)
  {
    if (description is not null && !string.IsNullOrWhiteSpace(description))
    {
      var matchedTransactions = await transactionService.SearchByDescriptionAsync(description);
      return Results.Ok(matchedTransactions);
    }
    var transactions = await transactionService.GetAllAsync();
    return Results.Ok(transactions);
  }

  internal static async Task<IResult> GetTransactionById(
    long id, ITransactionService transactionService)
  {
    var transaction = await transactionService.GetByIdAsync(id);
    return transaction is not null ? Results.Ok(transaction) : Results.NotFound();
  }

  internal static async Task<IResult> UpdateTransactionAsync(
    long id, Transaction transaction,
    ITransactionService transactionService,IValidator<Transaction> validator)
  {
    transaction.Id = id;
    var validationResult = await validator.ValidateAsync(transaction);
    if (!validationResult.IsValid)
    {
      return Results.BadRequest(validationResult.Errors);
    }

    var updated = await transactionService.UpdateAsync(transaction);
    return updated ? Results.Ok(transaction) : Results.NotFound();
  }

  internal static async Task<IResult> DeleteTransactionAsync(
    long id, ITransactionService transactionService)
  {
    var deleted = await transactionService.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
  }

  public static void AddServices(IServiceCollection services,
    IConfiguration configuration)
  {
    services.AddSingleton<ITransactionService, TransactionService>();
  }
}
