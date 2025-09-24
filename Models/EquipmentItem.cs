namespace gutv_booker.Models
{
    public class EquipmentItem
    {
        public int Id { get; set; }
        public int EquipmentTypeId { get; set; }
        public string InventoryNumber { get; set; } = "";
        public bool Available { get; set; } = true;
    }
}