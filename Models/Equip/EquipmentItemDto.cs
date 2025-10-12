namespace gutv_booker.Models;

public class EquipmentItemDto
{
    public int Id { get; set; }
    public int EquipmentTypeId { get; set; }
    public string InventoryNumber { get; set; } = string.Empty;
    public bool Available { get; set; }
}