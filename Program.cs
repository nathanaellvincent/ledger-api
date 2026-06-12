using System.Text;
using LedgerApi.Data;
using LedgerApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidateIssuer  = false,
            ValidateAudience = false,
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/", () => Results.Content("""
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>ledger-api</title>
<style>
*,*::before,*::after{box-sizing:border-box;margin:0;padding:0}
body{background:#08111f;color:#e2e8f0;font-family:system-ui,-apple-system,sans-serif;min-height:100vh;display:flex;flex-direction:column}
main{max-width:680px;margin:0 auto;padding:4rem 2rem;flex:1}
h1{font-size:2rem;font-weight:700;color:#fff;letter-spacing:-0.02em}
.tag{display:inline-block;margin-top:.6rem;padding:.2rem .7rem;background:#0f2a1a;color:#34d399;border-radius:9999px;font-size:.72rem;font-weight:600;letter-spacing:.06em;text-transform:uppercase}
.lead{color:#94a3b8;margin-top:1rem;line-height:1.65;font-size:.95rem}
h2{font-size:.7rem;font-weight:600;text-transform:uppercase;letter-spacing:.1em;color:#475569;margin:2.5rem 0 .75rem}
.row{display:flex;align-items:baseline;gap:.9rem;padding:.65rem 1rem;border-radius:8px;margin-bottom:.4rem;background:#0d1b2e}
.m{font-family:'SF Mono','Fira Code',monospace;font-size:.75rem;font-weight:700;min-width:3rem}
.m.post{color:#34d399}.m.get{color:#60a5fa}
.p{font-family:'SF Mono','Fira Code',monospace;font-size:.85rem;color:#e2e8f0;flex:1}
.d{font-size:.78rem;color:#475569}
pre{background:#0d1b2e;border:1px solid #1e293b;border-radius:8px;padding:1.2rem 1.4rem;font-family:'SF Mono','Fira Code',monospace;font-size:.8rem;color:#94a3b8;overflow-x:auto;line-height:1.7}
.c{color:#60a5fa}.s{color:#34d399}
a{color:#34d399;text-decoration:none}a:hover{text-decoration:underline}
footer{border-top:1px solid #0d1b2e;padding:1.4rem 2rem;text-align:center;font-size:.78rem;color:#334155}
</style>
</head>
<body>
<main>
  <h1>ledger-api</h1>
  <span class="tag">C# &middot; ASP.NET Core &middot; JWT</span>
  <p class="lead">A personal finance REST API with JWT authentication, transaction tracking, and budget summaries. Built with ASP.NET Core 8, Entity Framework Core, and SQLite - demonstrating clean controller/service separation, repository pattern, and stateless auth.</p>

  <h2>Auth endpoints</h2>
  <div class="row"><span class="m post">POST</span><span class="p">/api/auth/register</span><span class="d">Register a new account</span></div>
  <div class="row"><span class="m post">POST</span><span class="p">/api/auth/login</span><span class="d">Login and receive a JWT token</span></div>

  <h2>Transaction endpoints (JWT required)</h2>
  <div class="row"><span class="m post">POST</span><span class="p">/api/transactions</span><span class="d">Create a transaction (income or expense)</span></div>
  <div class="row"><span class="m get">GET</span><span class="p">/api/transactions</span><span class="d">List all transactions for the current user</span></div>
  <div class="row"><span class="m get">GET</span><span class="p">/api/transactions/summary</span><span class="d">Balance, total income, total expenses</span></div>

  <h2>Quick example</h2>
  <pre><span class="c">curl</span> -X POST https://vn-ledger-api.onrender.com/api/auth/register \
  -H <span class="s">"Content-Type: application/json"</span> \
  -d <span class="s">'{"username":"demo","password":"demo1234"}'</span></pre>

  <h2>Source</h2>
  <p><a href="https://github.com/nathanaellvincent/ledger-api">github.com/nathanaellvincent/ledger-api</a></p>
</main>
<footer>Built by <a href="https://vincentnathanael.com">Vincent Nathanael</a></footer>
</body>
</html>
""", "text/html"));

app.Run();
