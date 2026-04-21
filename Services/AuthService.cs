using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LedgerApi.Data;
using LedgerApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LedgerApi.Services;

public interface IAuthService
{
    Task<string?> RegisterAsync(string username, string password);
    Task<string?> LoginAsync(string username, string password);
}

public class AuthService(AppDbContext db, IConfiguration config) : IAuthService
{
    public async Task<string?> RegisterAsync(string username, string password)
    {
        if (await db.Users.AnyAsync(u => u.Username == username))
            return null;

        var user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return GenerateToken(user);
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return GenerateToken(user);
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(
            config.GetValue<int>("Jwt:ExpiresInHours"));

        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.Name, user.Username),
                     new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())],
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
