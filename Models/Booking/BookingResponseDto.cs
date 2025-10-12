namespace gutv_booker.Models;

public class BookingResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<BookingItemDto> Items { get; set; } = new();

    public List<string> Warnings { get; set; } = new();
    public string Comment { get; set; } = string.Empty;
}