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

    private async Task<(bool Success, EquipmentItem? Item)> GetAvailableItem(int equipmentTypeId, DateTime start, DateTime end,
        HashSet<int> alreadySelectedIds)
    {
        var item = await _context.EquipmentItems
            .Where(e => e.EquipmentTypeId == equipmentTypeId)
            .Where(e => !alreadySelectedIds.Contains(e.Id))
            .Where(e => !_context.BookingItems.Any(bi =>
                bi.EquipmentItemId == e.Id &&
                (bi.Booking.Status == Booking.BookingStatus.Pending ||
                 bi.Booking.Status == Booking.BookingStatus.Approved) &&
                start < bi.EndDate && end > bi.StartDate
            ))
            .FirstOrDefaultAsync();

        if (item == null)
            return (false, null);

        return (true, item);
    }

    private async Task<List<BookingResponseDto>> BookingToDto(List<Booking> bookings, int? equipmentItemId = null)
    {
        var allItemIds = bookings.SelectMany(b => b.BookingItems.Select(bi => bi.EquipmentItemId)).ToList();
        var equipmentItems = await _context.EquipmentItems
            .Where(e => allItemIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        if (equipmentItems.Count == 0)
            return new List<BookingResponseDto>();

        return bookings.Select(b => new BookingResponseDto
        {
            Id = b.Id,
            UserId = b.UserId,
            Name = b.Name,
            CreationDate = b.CreationDate,
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            Status = b.Status.ToString(),
            Comment = b.Comment,
            Items = b.BookingItems
                .Where(bi => equipmentItemId == null || bi.EquipmentItemId == equipmentItemId)
                .Select(bi => new BookingItemDto
                {
                    Id = bi.Id,
                    EquipmentItemId = bi.EquipmentItemId,
                    InventoryNumber = equipmentItems[bi.EquipmentItemId].InventoryNumber,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    IsReturned = bi.IsReturned
                }).ToList()
        }).ToList();
    }

    public async Task<(bool Success, BookingResponseDto? Booking, List<string>? Warnings)> CreateBooking(CreateBookingRequestDto request, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return (false, null, new List<string> { $"Пользователь с Id {userId} не найден" });

        if (request.Start >= request.End)
            return (false, null, new List<string> { "Дата начала должна быть меньше даты окончания" });

        if (request.EquipmentTypeIds == null || !request.EquipmentTypeIds.Any())
            return (false, null, new List<string> { "Не указано оборудование для бронирования" });

        var warnings = new List<string>();
        if ((request.Start - DateTime.UtcNow).TotalDays < 3)
            warnings.Add("Бронирование создаётся меньше чем за 3 дня до начала");

        var booking = new Booking
        {
            UserId = userId,
            Name = request.Name,
            StartDate = request.Start,
            EndDate = request.End,
            Status = Booking.BookingStatus.Pending,
            Comment = request.Comment
        };

        var selectedItemIds = new HashSet<int>();
        bool hasOsnovaItem = false;
        bool hasRoninItem = false;

        var equipmentTypes = await _context.EquipmentTypes
            .Where(et => request.EquipmentTypeIds.Contains(et.Id))
            .ToDictionaryAsync(et => et.Id);

        foreach (var typeId in request.EquipmentTypeIds)
        {
            var (itemSuccess, item) = await GetAvailableItem(typeId, request.Start, request.End, selectedItemIds);
            if (!itemSuccess || item == null)
                return (false, null, new List<string> { $"Нет доступных предметов для типа оборудования {typeId}" });

            selectedItemIds.Add(item.Id);

            booking.BookingItems.Add(new BookingItem
            {
                EquipmentItemId = item.Id,
                StartDate = request.Start,
                EndDate = request.End,
                IsReturned = false
            });

            if (equipmentTypes.TryGetValue(item.EquipmentTypeId, out var eqType))
            {
                if (eqType.Osnova) hasOsnovaItem = true;
                if (eqType.Ronin) hasRoninItem = true;
            }
        }

        if (hasOsnovaItem && !user.Osnova)
            warnings.Add("В бронировании есть оборудование только для основы");

        if (hasRoninItem && !user.Ronin)
            return (false, null, new List<string> { "У вас нет разрешения на Ronin" });

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var result = await BookingToDto(new List<Booking> { booking });
        var response = result.FirstOrDefault();
        if (response != null)
            response.Warnings = warnings;

        return (true, response, warnings);
    }

    public async Task<(bool Success, BookingResponseDto? Booking)> GetBookingById(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.BookingItems)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null || !booking.BookingItems.Any())
            return (false, null);

        var result = await BookingToDto(new List<Booking> { booking });
        return (true, result.FirstOrDefault());
    }

    public async Task<List<BookingResponseDto>> GetBookingsByUser(int userId)
    {
        var bookings = await _context.Bookings
            .Where(b => b.UserId == userId)
            .Include(b => b.BookingItems)
            .ToListAsync();

        return await BookingToDto(bookings);
    }

    public async Task<List<BookingResponseDto>> GetBookingsByEquipmentItem(int equipmentItemId)
    {
        var bookings = await _context.Bookings
            .Where(b => b.BookingItems.Any(bi => bi.EquipmentItemId == equipmentItemId))
            .Include(b => b.BookingItems)
            .ToListAsync();

        return await BookingToDto(bookings, equipmentItemId);
    }

    public async Task<List<BookingResponseDto>> GetBookingsByStatus(Booking.BookingStatus status)
    {
        var bookings = await _context.Bookings
            .Where(b => b.Status == status)
            .Include(b => b.BookingItems)
            .ToListAsync();

        return await BookingToDto(bookings);
    }

    public async Task<List<BookingResponseDto>> GetBookingsByInventoryNumber(string inventoryNumber)
    {
        if (string.IsNullOrWhiteSpace(inventoryNumber))
            return new List<BookingResponseDto>();

        var equipmentItem = await _context.EquipmentItems
            .FirstOrDefaultAsync(e => e.InventoryNumber.ToLower() == inventoryNumber.ToLower());

        if (equipmentItem == null)
            return new List<BookingResponseDto>();

        var bookings = await _context.Bookings
            .Where(b => b.BookingItems.Any(bi => bi.EquipmentItemId == equipmentItem.Id))
            .Include(b => b.BookingItems)
            .ToListAsync();

        return await BookingToDto(bookings, equipmentItem.Id);
    }

    public async Task<bool> ApproveBooking(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null) return false;

        booking.Status = Booking.BookingStatus.Approved;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelBooking(int bookingId, int currentUserId, bool isAdmin)
    {
        var booking = await _context.Bookings
            .Include(b => b.BookingItems)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null) return false;
        if (!isAdmin && booking.UserId != currentUserId) return false;

        _context.BookingItems.RemoveRange(booking.BookingItems);
        _context.Bookings.Remove(booking);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteBooking(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null) return false;

        booking.Status = Booking.BookingStatus.Completed;
        await _context.SaveChangesAsync();
        return true;
    }
}