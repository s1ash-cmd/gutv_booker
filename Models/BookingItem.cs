using System.Text.Json.Serialization;

namespace gutv_booker.Models
{
    public class BookingItem
    {
        public int Id { get; set; }

        public int BookingId { get; set; }
        [JsonIgnore]
        public Booking Booking { get; set; } = null!;

        public int EquipmentItemId { get; set; }
        [JsonIgnore]
        public EquipmentItem EquipmentItem { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsReturned { get; set; } = false;
    }
}