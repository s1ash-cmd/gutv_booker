namespace gutv_booker.Models;

public class GetAvailableItemsRequestDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public int? ModelId { get; set; }
}
