namespace gutv_booker.Models;

public class EquipmentItemReportDto
{
    public int Id { get; set; }
    public string InventoryNumber { get; set; } = "";
    public bool Available { get; set; }

    public string? TypeName { get; set; }
    public string? TypeCategory { get; set; }
}