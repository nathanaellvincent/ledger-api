namespace LedgerApi.Models;

public enum TransactionKind { Income, Expense }

public class Transaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal Amount { get; set; }
    public TransactionKind Kind { get; set; }
    public string Category { get; set; } = "";
    public string? Note { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
}
