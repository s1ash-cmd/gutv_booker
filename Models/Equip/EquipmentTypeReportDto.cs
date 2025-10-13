namespace gutv_booker.Models;

public class EquipmentTypeReportDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public bool Osnova { get; set; }
    public bool Ronin { get; set; }

    public Dictionary<string, object>? Attributes { get; set; }

    public List<EquipmentItemReportDto> Items { get; set; } = new();
}