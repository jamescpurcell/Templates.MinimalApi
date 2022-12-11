namespace Templates.MinimalApi.Models;

public class Transaction
{
  public long Id { get; set; }
  public decimal Amount { get; set; } = default!;
  public string Description { get; set; } = default!;
  public string Type { get; set; } = default!;
  public string IpAddressV4 { get; set; } = default!;
  public string IpAddressV6 { get; set; } = default!;
  public DateTime TransactionDate { get; set; }
}
