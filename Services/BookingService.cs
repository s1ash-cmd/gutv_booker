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

    public static BookingResponseDto BookingToResponseDto(Booking booking) => new BookingResponseDto
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
            StartDate = bi.StartDate,
            EndDate = bi.EndDate,
            IsReturned = bi.IsReturned
        }).ToList()
    };


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

        if (request.StartTime >= request.EndTime)
            throw new ArgumentException("Время начала должно быть раньше времени окончания");

        if (!request.Equipment.Any())
            throw new ArgumentException("Не указано оборудование для бронирования");

        var warnings = new Dictionary<string, object>();
        if ((request.StartTime - DateTime.UtcNow).TotalDays < 3)
            warnings["Предупреждение"] = "Бронирование создается меньше чем за 3 дня до начала";

        var booking = CreateDtoToBooking(request);
        booking.CreationTime = DateTime.UtcNow;
        booking.Warnings = warnings;
        booking.UserId = user.Id;

        var bookingItems = new List<BookingItem>();

        foreach (var item in request.Equipment)
        {
            if (item.Quantity <= 0)
                throw new ArgumentException($"Некорректное количество для модели {item.ModelId}: {item.Quantity}");

            var availableItems = await GetAvailableItems(
                item.ModelId, request.StartTime, request.EndTime, item.Quantity);

            if (availableItems.Count < item.Quantity)
            {
                var model = await _context.EquipmentModels.FindAsync(item.ModelId);
                var modelName = model?.Name ?? "Неизвестная модель";
                throw new InvalidOperationException(
                    $"Недостаточно доступного оборудования: {modelName}. " +
                    $"Запрошено: {item.Quantity}, доступно: {availableItems.Count}");
            }

            foreach (var equipmentItem in availableItems)
            {
                bookingItems.Add(new BookingItem
                {
                    EquipmentItemId = equipmentItem.Id,
                    StartDate = request.StartTime,
                    EndDate = request.EndTime,
                    IsReturned = false
                });
            }
        }

        booking.BookingItems = bookingItems;

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var createdBooking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.BookingItems)
            .ThenInclude(bi => bi.EquipmentItem)
            .FirstAsync(b => b.Id == booking.Id);

        return BookingToResponseDto(createdBooking);
    }
}