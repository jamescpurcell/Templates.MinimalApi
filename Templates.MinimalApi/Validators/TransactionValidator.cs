using FluentValidation;
using Templates.MinimalApi.Models;

namespace Templates.MinimalApi.Validators;

public class TransactionValidator : AbstractValidator<Transaction>
{
  public TransactionValidator()
  {
    /*RuleFor(book => book.Isbn)
      .Matches(@"^(?=(?:\D*\d){10}(?:(?:\D*\d){3})?$)[\d-]+$")
        .WithMessage("Value was not a valid ISBN-13");*/
    RuleFor(transaction => transaction.Id).GreaterThan(0);
    RuleFor(transaction => transaction.Amount).NotEmpty();
    RuleFor(transaction => transaction.Description).NotEmpty();
    RuleFor(transaction => transaction.Type).NotEmpty();
  }
}
