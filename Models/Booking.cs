namespace gutv_booker.Models
{
    public enum BookingStatus
    {
        Pending,
        Approved,
        Cancelled
    }
    public class Booking
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int EquipmentItemId { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;
    }
}