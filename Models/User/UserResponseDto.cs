namespace gutv_booker.Models;

public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Login { get; set; }
    public long? TelegramChatId { get; set; }
    public string? TelegramUsername { get; set; }
    public bool IsTelegramLinked { get; set; }
    public string Role { get; set; }
    public bool Banned { get; set; }
}
