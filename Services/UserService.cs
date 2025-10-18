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

    public static UserResponseDto UserToResponseDto(User user) => new UserResponseDto
    {
        Id = user.Id,
        Name = user.Name,
        TelegramId = user.TelegramId,
        Role = user.Role.ToString(),
        Banned = user.Banned
    };

    private User CreateDtoToUser(CreateUserRequestDto request)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(request.Password + salt));
        var passwordHash = Convert.ToBase64String(hashBytes);

        return new User
        {
            Login = request.Login,
            PasswordHash = passwordHash,
            Salt = salt,
            Name = request.Name,
            Role = request.Ronin ? User.UserRole.Ronin : User.UserRole.User,
            JoinYear = request.JoinYear
        };
    }

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Login.ToLower() == login.ToLower());
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
        return await _context.Users.FirstOrDefaultAsync(u =>
            u.RefreshToken == refreshToken && u.RefreshTokenExpiryTime > DateTime.UtcNow);
    }

    public async Task<UserResponseDto> CreateUser(CreateUserRequestDto request)
    {
        if (await _context.Users.AnyAsync(u => EF.Functions.ILike(u.Login, request.Login)))
            throw new InvalidOperationException("Пользователь с таким логином уже существует");

        var user = CreateDtoToUser(request);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return UserToResponseDto(user);
    }

    public async Task<List<UserResponseDto>> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        return users.Select(UserToResponseDto).ToList();
    }

    public async Task<UserResponseDto?> GetUserById(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;
        return UserToResponseDto(user);
    }

    public async Task<List<UserResponseDto>?> GetUsersByName(string namePart)
    {
        var users = await _context.Users
            .Where(u => EF.Functions.ILike(u.Name, $"%{namePart}%")).Select(u => UserService.UserToResponseDto(u)).ToListAsync();

        return users.Any() ? users : null;
    }

    public async Task<List<UserResponseDto>?> GetUserByRole(User.UserRole role)
    {
        var users = await _context.Users.Where(u => u.Role == role).Select(u => UserService.UserToResponseDto(u)).ToListAsync();

        return users.Any() ? users : null;
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

    public async Task<bool> GrantRonin(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        user.Role = User.UserRole.Ronin;
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