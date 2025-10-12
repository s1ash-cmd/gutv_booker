using System.Security.Cryptography;
using System.Text;
using gutv_booker.Data;
using gutv_booker.Models;
using Microsoft.EntityFrameworkCore;

namespace gutv_booker.Services;

public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    private static UserNoAuthDto ToDto(User user) => new UserNoAuthDto
    {
        Id = user.Id,
        Name = user.Name,
        TelegramId = user.TelegramId,
        Role = user.Role,
        Banned = user.Banned
    };

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Login.ToLower() == login.ToLower());
    }

    public async Task SaveRefreshTokenAsync(int userId, string refreshToken)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken
                                      && u.RefreshTokenExpiryTime > DateTime.UtcNow);
    }

    public async Task<UserNoAuthDto> CreateUserAsync(string login, string password, string name, DateOnly joinDate,
        string telegramId = "", bool ronin = false, User.UserRole role = User.UserRole.User)
    {
        if (await _context.Users.AnyAsync(u => u.Login.ToLower() == login.ToLower()))
            throw new InvalidOperationException($"Пользователь с логином '{login}' уже существует");

        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
        var passwordHash = Convert.ToBase64String(hashBytes);

        var user = new User
        {
            Login = login,
            PasswordHash = passwordHash,
            Salt = salt,
            Name = name,
            TelegramId = telegramId,
            Role = role,
            Banned = false,
            JoinDate = joinDate,
            Ronin = ronin
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserNoAuthDto
        {
            Id = user.Id,
            Name = user.Name,
            TelegramId = user.TelegramId,
            Role = user.Role,
            Banned = user.Banned,
            Osnova = user.Osnova,
            Ronin = user.Ronin
        };
    }

    public async Task<List<UserNoAuthDto>> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        return users.Select(ToDto).ToList();
    }

    public async Task<UserNoAuthDto?> GetUserById(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;
        return ToDto(user);
    }

    public async Task<List<UserNoAuthDto>> GetUsersByName(string namePart)
    {
        return await _context.Users
            .Where(u => u.Name.ToLower().Contains(namePart.ToLower()))
            .Select(u => ToDto(u))
            .ToListAsync();
    }

    public async Task<bool> BanUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        user.Banned = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnbanUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        user.Banned = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MakeAdmin(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        user.Role = User.UserRole.Admin;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MakeUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        user.Role = User.UserRole.User;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}