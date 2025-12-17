using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gutv_booker.Models
{
    public class Booking
    {
        public enum BookingStatus
        {
            Pending,
            Cancelled,
            Approved,
            Completed
        }

        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string Reason { get; set; } = "";
        public DateTime CreationTime { get; set; } = DateTime.UtcNow;

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public List<BookingItem> BookingItems { get; set; } = new();

        [JsonIgnore]
        public string WarningsJson { get; set; } = "{}";


        [NotMapped]
        public Dictionary<string, object> Warnings
        {
            get => string.IsNullOrEmpty(WarningsJson)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(WarningsJson)!;
            set => WarningsJson = JsonSerializer.Serialize(value);
        }

        public string? Comment { get; set; }
        public string? AdminComment { get; set; }
    }
}