namespace gutv_booker.Models;

public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string TelegramId { get; set; } = "";
    public User.UserRole Role { get; set; }
    public bool Banned { get; set; }
}