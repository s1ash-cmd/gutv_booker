using gutv_booker.Data;
using gutv_booker.Models;
using Microsoft.EntityFrameworkCore;

namespace gutv_booker.Services;

public class BookingService
{
    private readonly AppDbContext _context;

    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    private async Task<EquipmentItem> GetAvailableItem(int equipmentTypeId, DateTime start, DateTime end,
        HashSet<int> alreadySelectedIds)
    {
        var item = await _context.EquipmentItems
            .Where(e => e.EquipmentModelId == equipmentTypeId)
            .Where(e => !alreadySelectedIds.Contains(e.Id))
            .Where(e => !_context.BookingItems.Any(bi =>
                bi.EquipmentItemId == e.Id &&
                (bi.Booking.Status == Booking.BookingStatus.Pending ||
                 bi.Booking.Status == Booking.BookingStatus.Approved) &&
                start < bi.EndDate && end > bi.StartDate
            ))
            .FirstOrDefaultAsync();

        return item;
    }
}