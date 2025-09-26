namespace gutv_booker.Models
{
    public class Booking
    {
        public enum BookingStatus
        {
            Pending,
            Approved,
            Cancelled
        }

        public int Id { get; set; }

        public int UserId { get; set; }
        public int EquipmentItemId { get; set; }
        public string Comment { get; set; } = "";

        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public void ApproveBooking()
        {
            Status = BookingStatus.Approved;
        }

        public void CancelBooking()
        {
            Status = BookingStatus.Cancelled;
        }

        public bool ThreeDays()
        {
            return StartDate.Date >= DateTime.UtcNow.Date.AddDays(3);
        }

        public bool HasValidDates() => EndDate > StartDate;
    }
}