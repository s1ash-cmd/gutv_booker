using gutv_booker.Data;
using gutv_booker.Models;

namespace gutv_booker.Services;

public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> CreateUser(string telegramId, User.UserRole role = User.UserRole.User)
    {
        var user = new User
        {
            TelegramId = telegramId,
            Role = role,
            Banned = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
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

    public async Task<bool> CheckAdmin(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user != null && user.Role == User.UserRole.Admin;
    }
}
