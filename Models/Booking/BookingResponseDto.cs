namespace gutv_booker.Models;

public class BookingResponseDto
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<BookingItemDto> EquipmentModelIds { get; set; } = new();

    public Dictionary<string, object> Warnings { get; set; } = new();

    public string Comment { get; set; } = string.Empty;
    public string? AdminComment { get; set; }
}