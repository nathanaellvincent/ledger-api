namespace LedgerApi.Models;

public class Budget
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Category { get; set; } = "";
    public decimal LimitAmount { get; set; }
    public int Month { get; set; }  // 1–12
    public int Year { get; set; }
}
