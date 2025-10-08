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

        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string Name { get; set; } = "";
        public string TelegramId { get; set; } = "";
        public UserRole Role { get; set; } = UserRole.User;
        [DefaultValue(false)]
        public bool Banned { get; set; } = false;
    }

    public class UserDtoNoAuth
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string TelegramId { get; set; } = "";
        public User.UserRole Role { get; set; }
        public bool Banned { get; set; }
    }
}