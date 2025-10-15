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

        public string Name { get; set; } = "";
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<BookingItem> BookingItems { get; set; } = new();
        public string Comment { get; set; } = "";
        public string? AdminComment { get; set; }
    }
}