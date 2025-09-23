namespace gutv_booker.Models;

public class EquipmentItem
{
    public int Id { get; set; }

    public int EquipmentId { get; set; }

    public Equipment Equipment { get; set; } = null!;

    public string InvNumber { get; set; } = "";

    public bool IsAvailable { get; set; } = true;
}
