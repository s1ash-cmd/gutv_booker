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

    private static UserDtoNoAuth ToDto(User user) => new UserDtoNoAuth
    {
        Id = user.Id,
        Name = user.Name,
        TelegramId = user.TelegramId,
        Role = user.Role,
        Banned = user.Banned
    };

    public async Task<User> CreateUser(string login, string password, string name, string telegramId, User.UserRole role = User.UserRole.User)
    {
        if (await _context.Users.AnyAsync(u => u.Login == login))
            throw new InvalidOperationException($"Пользователь с логином '{login}' уже существует");

        var user = new User
        {
            Login = login,
            Password = password,
            Name = name,
            TelegramId = telegramId,
            Role = role,
            Banned = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<List<UserDtoNoAuth>> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        return users.Select(ToDto).ToList();
    }

    public async Task<UserDtoNoAuth?> GetUserById(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        return new UserDtoNoAuth
        {
            Id = user.Id,
            Name = user.Name,
            TelegramId = user.TelegramId,
            Role = user.Role,
            Banned = user.Banned
        };
    }

    public async Task<List<UserDtoNoAuth>> GetUsersByName(string namePart)
    {
        return await _context.Users
            .Where(u => u.Name.ToLower().Contains(namePart.ToLower()))
            .Select(u => new UserDtoNoAuth
            {
                Id = u.Id,
                Name = u.Name,
                TelegramId = u.TelegramId,
                Role = u.Role,
                Banned = u.Banned
            })
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