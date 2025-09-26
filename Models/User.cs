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
        public bool Banned { get; set; } = false;

        public void Ban()
        {
            Banned = true;
        }

        public void Unban()
        {
            Banned = false;
        }

        public bool CanBook() => !Banned;

        public bool IsAdmin()
        {
            return Role == UserRole.Admin;
        }

        public void PromoteToAdmin() => Role = UserRole.Admin;

    }
}