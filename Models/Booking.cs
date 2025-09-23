namespace gutv_booker.Models;

public class Booking
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public int EquipmentItemId { get; set; }

    public EquipmentItem EquipmentItem { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsOverlapping(Booking other)
    {
        return other != null && StartDate < other.EndDate && EndDate > other.StartDate;
    }

    //сделать проверку, что за 3 дня до брони, даты по порядку
}
