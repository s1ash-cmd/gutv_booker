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
            throw new InvalidOperationException($"Нет доступных предметов для типа оборудования {equipmentTypeId}");

        return item;
    }

    private async Task<List<BookingResponseDto>> BookingToDto(List<Booking> bookings, int? equipmentItemId = null)
    {
        var allItemIds = bookings.SelectMany(b => b.BookingItems.Select(bi => bi.EquipmentItemId)).ToList();
        var equipmentItems = await _context.EquipmentItems
            .Where(e => allItemIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        if (equipmentItems.Count == 0)
            throw new InvalidOperationException("Не найдено оборудования для указанных броней");

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

    public async Task<BookingResponseDto> CreateBooking(CreateBookingRequestDto request, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException($"Пользователь с Id {userId} не найден");

        if (request.Start >= request.End)
            throw new InvalidOperationException("Дата начала должна быть меньше даты окончания");

        if (request.EquipmentTypeIds == null || !request.EquipmentTypeIds.Any())
            throw new InvalidOperationException("Не указано оборудование для бронирования");

        var warnings = new List<string>();

        var minAdvanceDays = 3;
        if ((request.Start - DateTime.UtcNow).TotalDays < minAdvanceDays)
            warnings.Add($"Бронирование создаётся меньше чем за {minAdvanceDays} дня до начала");

        var booking = new Booking
        {
            UserId = userId,
            Name = request.Name,
            StartDate = request.Start,
            EndDate = request.End,
            Status = Booking.BookingStatus.Pending,
            Comment = request.Comment,
        };

        var selectedItemIds = new HashSet<int>();
        bool hasOsnovaItem = false;
        bool hasRoninItem = false;

        var equipmentTypes = await _context.EquipmentTypes
            .Where(et => request.EquipmentTypeIds.Contains(et.Id))
            .ToDictionaryAsync(et => et.Id);

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

            if (equipmentTypes.TryGetValue(item.EquipmentTypeId, out var eqType))
            {
                if (eqType.Osnova)
                    hasOsnovaItem = true;

                if (eqType.Ronin)
                    hasRoninItem = true;
            }
        }

        if (hasOsnovaItem && !user.Osnova)
            warnings.Add("В бронировании есть оборудование только для основы");

        if (hasRoninItem && !user.Ronin)
            throw new InvalidOperationException("У вас нет разрешения на Ronin");

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var result = await BookingToDto(new List<Booking> { booking });
        var response = result.First();
        response.Warnings = warnings;

        return response;
    }


    public async Task<BookingResponseDto> GetBookingById(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.BookingItems)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
            throw new InvalidOperationException($"Бронь с Id {id} не найдена");

        if (!booking.BookingItems.Any())
            throw new InvalidOperationException($"У брони с Id {id} нет связанных предметов");

        var result = await BookingToDto(new List<Booking> { booking });
        return result.First();
    }

    public async Task<List<BookingResponseDto>> GetBookingsByUser(int userId)
    {
        var bookings = await _context.Bookings
            .Where(b => b.UserId == userId)
            .Include(b => b.BookingItems)
            .ToListAsync();

        if (!bookings.Any())
            throw new InvalidOperationException($"Не найдено броней для пользователя с Id {userId}");

        return await BookingToDto(bookings);
    }

    public async Task<List<BookingResponseDto>> GetBookingsByEquipmentItem(int equipmentItemId)
    {
        var bookings = await _context.Bookings
            .Where(b => b.BookingItems.Any(bi => bi.EquipmentItemId == equipmentItemId))
            .Include(b => b.BookingItems)
            .ToListAsync();

        if (!bookings.Any())
            throw new InvalidOperationException($"Не найдено броней, содержащих предмет с Id {equipmentItemId}");

        return await BookingToDto(bookings);
    }

    public async Task<List<BookingResponseDto>> GetBookingsByStatus(Booking.BookingStatus status)
    {
        var bookings = await _context.Bookings
            .Where(b => b.Status == status)
            .Include(b => b.BookingItems)
            .ToListAsync();

        if (!bookings.Any())
            throw new InvalidOperationException($"Не найдено броней со статусом {status}");

        return await BookingToDto(bookings);
    }

    public async Task<List<BookingResponseDto>> GetBookingsByInventoryNumber(string inventoryNumber)
    {
        if (string.IsNullOrWhiteSpace(inventoryNumber))
            throw new InvalidOperationException("Инвентарный номер не может быть пустым");

        var equipmentItem = await _context.EquipmentItems
            .FirstOrDefaultAsync(e => e.InventoryNumber.ToLower() == inventoryNumber.ToLower());

        if (equipmentItem == null)
            throw new InvalidOperationException($"Оборудование с инвентарным номером '{inventoryNumber}' не найдено");

        var bookings = await _context.Bookings
            .Where(b => b.BookingItems.Any(bi => bi.EquipmentItemId == equipmentItem.Id))
            .Include(b => b.BookingItems)
            .ToListAsync();

        if (!bookings.Any())
            throw new InvalidOperationException($"Не найдено броней, связанных с предметом '{inventoryNumber}'");

        return await BookingToDto(bookings, equipmentItem.Id);
    }

    public async Task ApproveBooking(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null)
            throw new InvalidOperationException($"Бронь с Id {bookingId} не найдена");

        booking.Status = Booking.BookingStatus.Approved;
        await _context.SaveChangesAsync();
    }

    public async Task CancelBooking(int bookingId, int currentUserId, bool isAdmin)
    {
        var booking = await _context.Bookings
            .Include(b => b.BookingItems)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            throw new InvalidOperationException($"Бронь с Id {bookingId} не найдена");

        if (!isAdmin && booking.UserId != currentUserId)
            throw new UnauthorizedAccessException("Вы не можете удалить чужую бронь");

        _context.BookingItems.RemoveRange(booking.BookingItems);
        _context.Bookings.Remove(booking);

        await _context.SaveChangesAsync();
    }

    public async Task CompleteBooking(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null)
            throw new InvalidOperationException($"Бронь с Id {bookingId} не найдена");

        booking.Status = Booking.BookingStatus.Completed;
        await _context.SaveChangesAsync();
    }
}