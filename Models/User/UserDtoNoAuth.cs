namespace gutv_booker.Models;

public class UserNoAuthDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string TelegramId { get; set; } = "";
    public User.UserRole Role { get; set; }
    public bool Banned { get; set; }
    public bool Osnova {get; set;} = false;
    public bool Ronin {get; set; } = false;
}