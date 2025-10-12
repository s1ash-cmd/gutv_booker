namespace gutv_booker.Models;

public class CreateUserRequestDto
{
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
    public string Name { get; set; } = "";
    public string TelegramId { get; set; } = "";
    public DateOnly JoinDate { get; set; }
    public bool Ronin {get; set; } = false;
}