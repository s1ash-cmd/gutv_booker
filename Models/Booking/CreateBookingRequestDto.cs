namespace gutv_booker.Models;

public class CreateBookingRequestDto
{
    public int UserId { get; set; }

    public string Name { get; set; } = "";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public List<int> EquipmentTypeIds { get; set; } = new();
    public string Comment { get; set; } = "";
}
