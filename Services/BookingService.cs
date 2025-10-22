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
        if (user == null)
            return null;

        if (request.StartTime >= request.EndTime)
            return null;

        if (!request.Equipment.Any())
            return null;

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
                return null;

            var availableItems = await GetAvailableItems(item.ModelId, request.StartTime, request.EndTime, item.Quantity);
            if (availableItems.Count < item.Quantity)
                return null;

            bookingItems.AddRange(
                availableItems.Select(equipmentItem => new BookingItem
                {
                    EquipmentItemId = equipmentItem.Id,
                    StartDate = request.StartTime,
                    EndDate = request.EndTime,
                    IsReturned = false
                })
            );
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

    public async Task<BookingResponseDto?> GetBookingById(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.BookingItems)
            .ThenInclude(bi => bi.EquipmentItem)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null || !booking.BookingItems.Any())
            return null;

        return BookingToResponseDto(booking);
    }

    public async Task<List<BookingResponseDto>> GetBookingsByUser(int userId)
    {
        var bookings = await _context.Bookings
            .Where(b => b.UserId == userId)
            .Include(b => b.User)
            .Include(b => b.BookingItems)
            .ThenInclude(bi => bi.EquipmentItem)
            .ToListAsync();

        return bookings
            .Where(b => b.BookingItems.Any())
            .Select(BookingToResponseDto)
            .ToList();
    }

    public async Task<List<BookingResponseDto>> GetBookingsByEquipmentItem(int equipmentItemId)
    {
        var bookings = await _context.Bookings
            .Where(b => b.BookingItems.Any(bi => bi.EquipmentItemId == equipmentItemId))
            .Include(b => b.User)
            .Include(b => b.BookingItems)
            .ThenInclude(bi => bi.EquipmentItem)
            .ToListAsync();

        return bookings
            .Where(b => b.BookingItems.Any())
            .Select(BookingToResponseDto)
            .ToList();
    }

    public async Task<List<BookingResponseDto>> GetBookingsByStatus(Booking.BookingStatus status)
    {
        var bookings = await _context.Bookings
            .Where(b => b.Status == status)
            .Include(b => b.User)
            .Include(b => b.BookingItems)
            .ThenInclude(bi => bi.EquipmentItem)
            .ToListAsync();

        return bookings
            .Where(b => b.BookingItems.Any())
            .Select(BookingToResponseDto)
            .ToList();
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
            .Include(b => b.User)
            .Include(b => b.BookingItems)
            .ThenInclude(bi => bi.EquipmentItem)
            .ToListAsync();

        return bookings
            .Where(b => b.BookingItems.Any())
            .Select(BookingToResponseDto)
            .ToList();
    }

    public async Task<bool> ApproveBooking(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null)
            return false;

        booking.Status = Booking.BookingStatus.Approved;
        await _context.SaveChangesAsync();
        return true;
    }

    //public async Task<bool> CancelBooking(int bookingId, int currentUserId, bool isAdmin)
    //{
       
    //}


}