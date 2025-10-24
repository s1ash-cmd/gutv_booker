namespace gutv_booker.Models;

public class EqItemResponseDto
{
    public int Id { get; set; }
    public string InventoryNumber { get; set; } = "";
    public bool Available { get; set; }

    public string? ModelName { get; set; }
    public string? ModelCategory { get; set; }
}