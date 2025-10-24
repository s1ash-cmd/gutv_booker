using gutv_booker.Data;
using gutv_booker.Models;
using Microsoft.EntityFrameworkCore;
using static gutv_booker.Models.EquipmentModel;
using static gutv_booker.Models.User;

namespace gutv_booker.Services;

public class BookingService
{
    private readonly AppDbContext _context;

    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    private Booking CreateDtoToBooking(CreateBookingRequestDto request)
    {
        return new Booking
        {
            Reason = request.Reason,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = Booking.BookingStatus.Pending,
            Warnings = new Dictionary<string, object>(),
            BookingItems = new List<BookingItem>(),
            Comment = request.Comment
        };
    }

    public static BookingResponseDto BookingToResponseDto(Booking booking)
    {
        return new BookingResponseDto
        {
            Id = booking.Id,
            Reason = booking.Reason,
            CreationTime = booking.CreationTime,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Status = booking.Status.ToString(),
            Comment = booking.Comment,
            AdminComment = booking.AdminComment,
            Warnings = booking.Warnings,
            UserName = booking.User?.Login ?? string.Empty,

            EquipmentModelIds = booking.BookingItems.Select(bi => new BookingItemDto
            {
                Id = bi.Id,
                EquipmentItemId = bi.EquipmentItemId,
                InventoryNumber = bi.EquipmentItem?.InventoryNumber ?? string.Empty,
                ModelName = bi.EquipmentItem?.EquipmentModel?.Name ?? string.Empty,
                StartDate = bi.StartDate,
                EndDate = bi.EndDate,
                IsReturned = bi.IsReturned
            }).ToList()
        };
    }

    private async Task<List<EquipmentItem>> GetAvailableItems(
        int equipmentModelId, DateTime start, DateTime end, int requiredCount)
    {
        var items = await _context.EquipmentItems
            .Include(e => e.EquipmentModel)
            .Where(e => e.EquipmentModelId == equipmentModelId)
            .Where(e => e.Available)
            .Where(e => !e.BookingItems.Any(bi =>
                (bi.Booking.Status == Booking.BookingStatus.Pending ||
                 bi.Booking.Status == Booking.BookingStatus.Approved) &&
                start < bi.EndDate && end > bi.StartDate))
            .Take(requiredCount + 1)
            .ToListAsync();

        return items.Take(requiredCount).ToList();
    }

    public async Task<BookingResponseDto> CreateBooking(CreateBookingRequestDto request, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Пользователь не найден");

        if (request.StartTime >= request.EndTime)
            throw new ArgumentException("Дата начала должна быть раньше даты окончания");

        if (request.Equipment == null || !request.Equipment.Any())
            throw new ArgumentException("Не выбрано оборудование для бронирования");

        var warnings = new Dictionary<string, object>();
        if ((request.StartTime - DateTime.UtcNow).TotalDays < 3)
            warnings["Неверная дата"] = "Бронирование создается меньше чем за 3 дня";

        var booking = CreateDtoToBooking(request);
        booking.CreationTime = DateTime.UtcNow;
        booking.UserId = user.Id;

        var bookingItems = new List<BookingItem>();
        foreach (var item in request.Equipment)
        {
            if (item.Quantity <= 0)
                throw new ArgumentException($"Количество для модели '{item.ModelName}' должно быть больше 0");

            var eqModel = await _context.EquipmentModels
                .FirstOrDefaultAsync(m => m.Name == item.ModelName);

            if (eqModel == null)
                throw new KeyNotFoundException($"Модель оборудования '{item.ModelName}' не найдена");

            switch (eqModel.Access)
            {
                case EquipmentAccess.Ronin:
                    if (user.Role < UserRole.Ronin)
                        throw new UnauthorizedAccessException("У вас нет доступа к Ronin");
                    break;
                case EquipmentAccess.Osnova:
                    if (user.Role < UserRole.Osnova)
                        warnings["Доступ"] = "Нет доступа к оборудованию основы";
                    break;
                case EquipmentAccess.User:
                    break;
            }

            booking.Warnings = warnings;

            var availableItems = await GetAvailableItems(eqModel.Id, request.StartTime, request.EndTime, item.Quantity);
            if (availableItems.Count < item.Quantity)
                throw new InvalidOperationException($"Недостаточно доступного оборудования модели '{item.ModelName}'");

            bookingItems.AddRange(availableItems.Select(equipmentItem => new BookingItem
            {
                EquipmentItemId = equipmentItem.Id,
                StartDate = request.StartTime,
                EndDate = request.EndTime,
                IsReturned = false
            }));
        }

        booking.BookingItems = bookingItems;

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var createdBooking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.BookingItems)
            .ThenInclude(bi => bi.EquipmentItem)
            .ThenInclude(ei => ei.EquipmentModel)
            .FirstAsync(b => b.Id == booking.Id);

        return BookingToResponseDto(createdBooking);
    }

    public async Task<BookingResponseDto> GetBookingById(int id)
    {
        var booking = await _context.Bookings
                          .Include(b => b.User)
                          .Include(b => b.BookingItems)
                          .ThenInclude(bi => bi.EquipmentItem)
                          .ThenInclude(ei => ei.EquipmentModel)
                          .FirstOrDefaultAsync(b => b.Id == id)
                      ?? throw new KeyNotFoundException($"Бронирование с ID {id} не найдено");

        if (!booking.BookingItems.Any())
            throw new InvalidOperationException("У бронирования нет связанных элементов оборудования");

        return BookingToResponseDto(booking);
    }

    public async Task<List<BookingResponseDto>> GetBookingsByUser(int userId)
    {
        var bookings = await _context.Bookings
            .Where(b => b.UserId == userId)
            .Include(b => b.User)
            .Include(b => b.BookingItems)
            .ThenInclude(bi => bi.EquipmentItem)
            .ThenInclude(ei => ei.EquipmentModel)
            .ToListAsync();

        if (!bookings.Any())
            throw new KeyNotFoundException($"У пользователя с ID {userId} нет бронирований");

        return bookings.Select(BookingToResponseDto).ToList();
    }

    public async Task<List<BookingResponseDto>> GetBookingsByEquipmentItem(int equipmentItemId)
    {
        var bookings = await _context.Bookings
            .Where(b => b.BookingItems.Any(bi => bi.EquipmentItemId == equipmentItemId))
            .Include(b => b.User)
            .Include(b => b.BookingItems)
            .ThenInclude(bi => bi.EquipmentItem)
            .ThenInclude(ei => ei.EquipmentModel)
            .ToListAsync();

        if (!bookings.Any())
            throw new KeyNotFoundException($"Не найдено бронирований для оборудования с ID {equipmentItemId}");

        return bookings.Select(BookingToResponseDto).ToList();
    }

    public async Task<List<BookingResponseDto>> GetBookingsByStatus(Booking.BookingStatus status)
    {
        var bookings = await _context.Bookings
            .Where(b => b.Status == status)
            .Include(b => b.User)
            .Include(b => b.BookingItems)
            .ThenInclude(bi => bi.EquipmentItem)
            .ThenInclude(ei => ei.EquipmentModel)
            .ToListAsync();

        if (!bookings.Any())
            throw new KeyNotFoundException($"Нет бронирований со статусом {status}");

        return bookings.Select(BookingToResponseDto).ToList();
    }

    public async Task<List<BookingResponseDto>> GetBookingsByInventoryNumber(string inventoryNumber)
    {
        if (string.IsNullOrWhiteSpace(inventoryNumber))
            throw new ArgumentException("Инвентарный номер не может быть пустым");

        var equipmentItem = await _context.EquipmentItems
                                .FirstOrDefaultAsync(e => e.InventoryNumber.ToLower() == inventoryNumber.ToLower())
                            ?? throw new KeyNotFoundException(
                                $"Оборудование с инвентарным номером {inventoryNumber} не найдено");

        var bookings = await _context.Bookings
            .Where(b => b.BookingItems.Any(bi => bi.EquipmentItemId == equipmentItem.Id))
            .Include(b => b.User)
            .Include(b => b.BookingItems)
            .ThenInclude(bi => bi.EquipmentItem)
            .ThenInclude(ei => ei.EquipmentModel)
            .ToListAsync();

        if (!bookings.Any())
            throw new KeyNotFoundException(
                $"Нет бронирований для оборудования с инвентарным номером {inventoryNumber}");

        return bookings.Select(BookingToResponseDto).ToList();
    }

    public async Task<bool> ApproveBooking(int bookingId, string? adminComment = null)
    {
        var booking = await _context.Bookings.FindAsync(bookingId)
                      ?? throw new KeyNotFoundException($"Бронирование с ID {bookingId} не найдено");

        booking.Status = Booking.BookingStatus.Approved;

        if (!string.IsNullOrWhiteSpace(adminComment)) booking.AdminComment = adminComment;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteBooking(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId)
                      ?? throw new KeyNotFoundException($"Бронирование с ID {bookingId} не найдено");

        booking.Status = Booking.BookingStatus.Completed;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelBooking(int bookingId, int userId, bool isAdmin, string? adminComment = null)
    {
        var booking = await _context.Bookings
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            throw new KeyNotFoundException($"Бронирование с ID {bookingId} не найдено");

        if (!isAdmin && booking.UserId != userId)
            throw new UnauthorizedAccessException("Вы не можете отменить чужое бронирование");

        if (booking.Status == Booking.BookingStatus.Cancelled)
            throw new InvalidOperationException("Это бронирование уже отменено");

        booking.Status = Booking.BookingStatus.Cancelled;

        if (isAdmin && !string.IsNullOrWhiteSpace(adminComment))
            booking.AdminComment = adminComment;

        await _context.SaveChangesAsync();
        return true;
    }
}