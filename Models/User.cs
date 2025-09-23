namespace gutv_booker.Models;

public class User
{
    public int Id { get; set; }

    public string TelegramId { get; set; } = "";

    public string Role { get; set; } = "User";

    public bool Banned { get; set; } = false;
}
