namespace gutv_booker.Models
{
    public enum UserRole
    {
        User,
        Admin
    }

    public class User
    {
        public int Id { get; set; }
        public string TelegramId { get; set; } = "";
        public UserRole Role { get; set; }
        public bool Banned { get; set; } = false;
    }
}