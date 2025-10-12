namespace gutv_booker.Models
{
    public class Booking
    {
        public enum BookingStatus
        {
            Pending,
            Approved,
            Cancelled,
            Rejected,
            Completed
        }

        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string Comment { get; set; } = "";
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<BookingItem> BookingItems { get; set; } = new();


        public class BookingItemDto
        {
            public int Id { get; set; }
            public int EquipmentItemId { get; set; }
            public string InventoryNumber { get; set; } = string.Empty;
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool IsReturned { get; set; }
        }

        public class CreateBookingRequestDto
        {
            public int UserId { get; set; }
            public string Comment { get; set; } = "";

            public DateTime Start { get; set; }
            public DateTime End { get; set; }

            public List<int> EquipmentTypeIds { get; set; } = new();
        }

        public class BookingResponseDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string Comment { get; set; } = string.Empty;
            public DateTime CreationDate { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Status { get; set; } = string.Empty;
            public List<BookingItemDto> Items { get; set; } = new();
        }
    }
}