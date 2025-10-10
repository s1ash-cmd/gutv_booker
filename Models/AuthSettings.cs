namespace gutv_booker.Models
{
    public class AuthSettings
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpireMinutes { get; set; }
        public int RefreshTokenExpireDays { get; set; }
        public string Key { get; set; } = string.Empty;
    }
}