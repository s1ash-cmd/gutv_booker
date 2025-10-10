namespace gutv_booker.Models
{
    public class EquipmentItem
    {
        public int Id { get; set; }
        public int EquipmentTypeId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public bool Available { get; set; } = true;

        public void MarkAsUnavailable()
        {
            Available = false;
        }

        public void MarkAsAvailable()
        {
            Available = true;
        }

        public bool IsAvailable(List<Booking> bookings, DateTime start, DateTime end)
        {
            return Available && !bookings.Any(b =>
                b.EquipmentItemId == Id &&
                b.Status == Booking.BookingStatus.Approved &&
                start < b.EndDate && end > b.StartDate
            );
        }
    }
}