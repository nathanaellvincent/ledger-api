using LedgerApi.Data;
using LedgerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LedgerApi.Services;

public record TransactionDto(int Id, decimal Amount, TransactionKind Kind,
    string Category, string? Note, DateTime Date);

public record MonthlySummary(string Category, decimal Spent, decimal? BudgetLimit,
    bool OverBudget);

public interface ITransactionService
{
    Task<TransactionDto> AddAsync(int userId, decimal amount, TransactionKind kind,
        string category, string? note, DateTime? date);
    Task<IReadOnlyList<TransactionDto>> GetByUserAsync(int userId,
        int? month = null, int? year = null);
    Task<IReadOnlyList<MonthlySummary>> GetSummaryAsync(int userId, int month, int year);
    Task<bool> DeleteAsync(int userId, int transactionId);
}

public class TransactionService(AppDbContext db) : ITransactionService
{
    public async Task<TransactionDto> AddAsync(int userId, decimal amount,
        TransactionKind kind, string category, string? note, DateTime? date)
    {
        var tx = new Transaction
        {
            UserId = userId,
            Amount = amount,
            Kind = kind,
            Category = category,
            Note = note,
            Date = date?.ToUniversalTime() ?? DateTime.UtcNow,
        };
        db.Transactions.Add(tx);
        await db.SaveChangesAsync();
        return ToDto(tx);
    }

    public async Task<IReadOnlyList<TransactionDto>> GetByUserAsync(int userId,
        int? month = null, int? year = null)
    {
        var query = db.Transactions.Where(t => t.UserId == userId);
        if (month.HasValue) query = query.Where(t => t.Date.Month == month.Value);
        if (year.HasValue)  query = query.Where(t => t.Date.Year == year.Value);

        return await query
            .OrderByDescending(t => t.Date)
            .Select(t => ToDto(t))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<MonthlySummary>> GetSummaryAsync(
        int userId, int month, int year)
    {
        var expenses = await db.Transactions
            .Where(t => t.UserId == userId
                     && t.Kind == TransactionKind.Expense
                     && t.Date.Month == month
                     && t.Date.Year == year)
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Spent = g.Sum(t => t.Amount) })
            .ToListAsync();

        var budgets = await db.Budgets
            .Where(b => b.UserId == userId && b.Month == month && b.Year == year)
            .ToDictionaryAsync(b => b.Category, b => b.LimitAmount);

        return expenses.Select(e => new MonthlySummary(
            e.Category,
            e.Spent,
            budgets.TryGetValue(e.Category, out var lim) ? lim : null,
            budgets.TryGetValue(e.Category, out var l) && e.Spent > l
        )).ToList();
    }

    public async Task<bool> DeleteAsync(int userId, int transactionId)
    {
        var tx = await db.Transactions
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId);
        if (tx is null) return false;
        db.Transactions.Remove(tx);
        await db.SaveChangesAsync();
        return true;
    }

    private static TransactionDto ToDto(Transaction t) =>
        new(t.Id, t.Amount, t.Kind, t.Category, t.Note, t.Date);
}
