public class CreateBookingRequestDto
{
    public string Reason { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Comment { get; set; }

    public List<EquipmentRequestItem> Equipment { get; set; } = new();
}

public class EquipmentRequestItem
{
    public string ModelName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}