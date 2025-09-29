using System.ComponentModel;

namespace gutv_booker.Models
{
    public class User
    {
        public enum UserRole
        {
            User,
            Admin
        }

        public int Id { get; set; }
        public string TelegramId { get; set; } = "";
        public UserRole Role { get; set; } = UserRole.User;
        [DefaultValue(false)]
        public bool Banned { get; set; } = false;
    }
}