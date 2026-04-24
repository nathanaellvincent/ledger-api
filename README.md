# ledger-api

Personal finance REST API — ASP.NET Core 8 with EF Core, JWT auth, and budget-vs-spend summaries.

## Why this project

Demonstrates C#/.NET patterns expected in UK fintech & enterprise roles:

| Pattern | Where |
|---|---|
| Minimal API bootstrap with `builder` | `Program.cs` |
| EF Core with SQLite + `Database.EnsureCreated()` | `Data/AppDbContext.cs`, `Program.cs` |
| Primary constructor DI (C# 12) | `Services/AuthService.cs`, all controllers |
| `record` types for DTOs | `Services/TransactionService.cs`, controllers |
| `[Authorize]` + `ClaimTypes.NameIdentifier` | `Controllers/TransactionController.cs` |
| BCrypt password hashing | `Services/AuthService.cs` |
| Interface-backed services for testability | `Services/IAuthService`, `ITransactionService` |

## API

```
POST   /api/auth/register      { username, password }
POST   /api/auth/login         { username, password }

POST   /api/transactions       { amount, kind: Income|Expense, category, note?, date? }
GET    /api/transactions?month=&year=
GET    /api/transactions/summary?month=&year=   → category totals vs budget limits
DELETE /api/transactions/{id}
```

All `/api/transactions` endpoints require `Authorization: Bearer <token>`.

## Run

Requires .NET 8 SDK.

```bash
dotnet run
# → http://localhost:5000
```

## Example

```bash
# Register
curl -X POST localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"vincent","password":"secret123"}'

# Add an expense
TOKEN="<token from above>"
curl -X POST localhost:5000/api/transactions \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"amount":450000,"kind":"Expense","category":"Food","note":"Nasi Padang"}'

# Monthly summary (shows overspend vs budget)
curl "localhost:5000/api/transactions/summary?month=6&year=2026" \
  -H "Authorization: Bearer $TOKEN"
```
