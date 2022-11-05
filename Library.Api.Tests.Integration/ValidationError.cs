namespace Library.Api.Tests.Integration;

public class ValidationError
{
  public string PropertyName { get; set; } = default!; //expect not null,
  public string ErrorMessage { get; set; } = default!; // and default to non-nullable version
}   
