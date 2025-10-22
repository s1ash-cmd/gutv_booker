namespace gutv_booker.Models;

public class BookingItemDto
{
    public int Id { get; set; }
    public int EquipmentItemId { get; set; }
    public string InventoryNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsReturned { get; set; }
}