namespace gutv_booker.Models
{
    public class EquipmentItem
    {
        public int Id { get; set; }
        public int EquipmentTypeId { get; set; }

        public EquipmentType EquipmentType { get; set; } = null!;

        public string InventoryNumber { get; set; } = string.Empty;
        public bool Available { get; set; } = true;

        public List<BookingItem> BookingItems { get; set; } = new();
    }

    public class EquipmentItemDto
    {
        public int Id { get; set; }
        public int EquipmentTypeId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public bool Available { get; set; }
    }
}