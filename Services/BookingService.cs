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

    private async Task<bool> IsItemAvailable(int equipmentItemId, DateTime start, DateTime end)
    {
        return !await _context.BookingItems
            .AnyAsync(bi =>
                bi.EquipmentItemId == equipmentItemId &&
                (bi.Booking.Status == Booking.BookingStatus.Pending ||
                 bi.Booking.Status == Booking.BookingStatus.Approved) &&
                start < bi.EndDate && end > bi.StartDate
            );
    }

    private async Task<EquipmentItem> GetAvailableItem(int equipmentTypeId, DateTime start, DateTime end,
        HashSet<int> alreadySelectedIds)
    {
        var items = await _context.EquipmentItems
            .Where(e => e.EquipmentTypeId == equipmentTypeId)
            .ToListAsync();

        foreach (var item in items)
        {
            if (alreadySelectedIds.Contains(item.Id))
                continue;

            if (await IsItemAvailable(item.Id, start, end))
                return item;
        }

        throw new InvalidOperationException($"Нет доступных предметов для типа оборудования {equipmentTypeId}");
    }

    public async Task<Booking.BookingResponseDto> CreateBooking(Booking.CreateBookingRequestDto request)
    {
        if (!await _context.Users.AnyAsync(u => u.Id == request.UserId))
            throw new InvalidOperationException($"Пользователь с Id {request.UserId} не найден");

        if (request.Start >= request.End)
            throw new InvalidOperationException("Дата начала должна быть меньше даты окончания");

        if (request.EquipmentTypeIds == null || !request.EquipmentTypeIds.Any())
            throw new InvalidOperationException("Не указано оборудование для бронирования");

        var booking = new Booking
        {
            UserId = request.UserId,
            Comment = request.Comment,
            StartDate = request.Start,
            EndDate = request.End,
            Status = Booking.BookingStatus.Pending
        };

        var selectedItemIds = new HashSet<int>();

        foreach (var typeId in request.EquipmentTypeIds)
        {
            var item = await GetAvailableItem(typeId, request.Start, request.End, selectedItemIds);
            selectedItemIds.Add(item.Id);

            booking.BookingItems.Add(new BookingItem
            {
                EquipmentItemId = item.Id,
                StartDate = request.Start,
                EndDate = request.End,
                IsReturned = false
            });
        }

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var itemIds = booking.BookingItems.Select(bi => bi.EquipmentItemId).ToList();
        var equipmentItems = await _context.EquipmentItems
            .Where(e => itemIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        return new Booking.BookingResponseDto
        {
            Id = booking.Id,
            UserId = booking.UserId,
            Comment = booking.Comment,
            CreationDate = booking.CreationDate,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            Status = booking.Status.ToString(),
            Items = booking.BookingItems.Select(bi => new Booking.BookingItemDto
            {
                Id = bi.Id,
                EquipmentItemId = bi.EquipmentItemId,
                InventoryNumber = equipmentItems[bi.EquipmentItemId].InventoryNumber,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                IsReturned = bi.IsReturned
            }).ToList()
        };
    }
}