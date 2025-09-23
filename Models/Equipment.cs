namespace gutv_booker.Models;

public class Equipment
{
    public int Id { get; set; }

    public string Brand { get; set; } = "";

    public string Model { get; set; } = "";

    public string Description { get; set; } = "";

    public ICollection<EquipmentItem> Items { get; set; } = new List<EquipmentItem>();
}
