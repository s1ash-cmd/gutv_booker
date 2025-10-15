using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace gutv_booker.Models
{
    public class User
    {
        public enum UserRole
        {
            User,
            Osnova,
            Ronin,
            Admin
        }

        public int Id { get; set; }
        public string Login { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string Salt { get; set; } = "";
        public string Name { get; set; } = "";
        public string? TelegramId { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
        public bool Banned { get; set; }

        [JsonIgnore]
        public int JoinYear { get; set; }

        [JsonIgnore]
        public string? RefreshToken { get; set; }

        [JsonIgnore]
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public bool CheckPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + Salt));
            var hash = Convert.ToBase64String(hashBytes);
            return hash == PasswordHash;
        }
    }
}