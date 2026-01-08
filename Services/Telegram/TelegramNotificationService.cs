using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using gutv_booker.Data;
using gutv_booker.Models;
using Microsoft.EntityFrameworkCore;

namespace gutv_booker.Services.Telegram;

public class TelegramNotificationService
{
    private readonly ITelegramBotClient _botClient;
    private readonly AppDbContext _context;
    private readonly ILogger<TelegramNotificationService> _logger;

    public TelegramNotificationService(
        ITelegramBotClient botClient,
        AppDbContext context,
        ILogger<TelegramNotificationService> logger)
    {
        _botClient = botClient;
        _context = context;
        _logger = logger;
    }

    public async Task NotifyAdminsNewBooking(Booking booking)
    {
        try
        {
            var admins = await _context.Users
                .Where(u => u.Role == User.UserRole.Admin && u.TelegramChatId.HasValue)
                .ToListAsync();

            var user = await _context.Users.FindAsync(booking.UserId);
            var bookingItems = await _context.BookingItems
                .Include(bi => bi.EquipmentItem)
                .ThenInclude(ei => ei.EquipmentModel)
                .Where(bi => bi.BookingId == booking.Id)
                .ToListAsync();

            var message = $"🆕 <b>Новое бронирование #{booking.Id}</b>\n\n" +
                          $"👤 <b>Пользователь:</b> {user?.Name} (@{user?.TelegramUsername ?? "-"})\n" +
                          $"📝 <b>Причина:</b> {booking.Reason}\n" +
                          $"📅 <b>Период:</b> {booking.StartTime:dd.MM.yyyy HH:mm} - {booking.EndTime:dd.MM.yyyy HH:mm}\n\n" +
                          $"📦 <b>Оборудование:</b>\n";

            foreach (var item in bookingItems)
            {
                message += $"   • {item.EquipmentItem?.EquipmentModel?.Name} ({item.EquipmentItem?.InventoryNumber})\n";
            }

            if (!string.IsNullOrEmpty(booking.Comment))
                message += $"\n💭 Комментарий: {booking.Comment}";

            if (booking.Warnings.Any())
                message += $"\n\n⚠️ Предупреждения: {string.Join(", ", booking.Warnings)}";

            message += $"\n\n⏳ Статус: <b>Ожидает подтверждения</b>";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✅ Подтвердить", $"booking:approve:{booking.Id}"),
                    InlineKeyboardButton.WithCallbackData("❌ Отклонить", $"booking:reject:{booking.Id}")
                }
            });

            foreach (var admin in admins)
            {
                await _botClient.SendMessage(
                    chatId: admin.TelegramChatId!.Value,
                    text: message,
                    parseMode: ParseMode.Html,
                    replyMarkup: keyboard);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка отправки уведомлений о бронировании #{booking.Id}");
        }
    }

    public async Task NotifyUserBookingStatusChanged(Booking booking, string oldStatus, string newStatus)
    {
        try
        {
            var user = await _context.Users.FindAsync(booking.UserId);

            if (user?.TelegramChatId == null)
            {
                _logger.LogInformation($"Пользователь {user?.Name} не привязал Telegram");
                return;
            }

            var statusEmoji = newStatus switch
            {
                "Approved" => "✅",
                "Completed" => "🏁",
                "Cancelled" => "❌",
                "Rejected" => "🚫"
            };

            var statusText = newStatus switch
            {
                "Approved" => "Одобрено",
                "Completed" => "Завершено",
                "Cancelled" => "Отменено",
                "Rejected" => "Отклонено"
            };

            var bookingItems = await _context.BookingItems
                .Include(bi => bi.EquipmentItem)
                .ThenInclude(ei => ei.EquipmentModel)
                .Where(bi => bi.BookingId == booking.Id)
                .ToListAsync();

            var message = $"{statusEmoji} <b>Изменение статуса бронирования #{booking.Id}</b>\n\n" +
                          $"Статус изменен: <s>{GetStatusName(oldStatus)}</s> → <b>{statusText}</b>\n\n" +
                          $"📝 Причина: {booking.Reason}\n" +
                          $"📅 Период: {booking.StartTime:dd.MM.yyyy HH:mm} - {booking.EndTime:dd.MM.yyyy HH:mm}\n\n" +
                          $"📦 Оборудование:\n";

            foreach (var item in bookingItems)
            {
                message += $"   • {item.EquipmentItem?.EquipmentModel?.Name} ({item.EquipmentItem?.InventoryNumber})\n";
            }

            if (!string.IsNullOrEmpty(booking.AdminComment))
                message += $"\n💬 Комментарий администратора: {booking.AdminComment}";

            await _botClient.SendMessage(
                chatId: user.TelegramChatId.Value,
                text: message,
                parseMode: ParseMode.Html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка отправки уведомления о смене статуса бронирования #{booking.Id}");
        }
    }

    private string GetStatusName(string status)
    {
        return status switch
        {
            "Pending" => "Ожидает",
            "Approved" => "Одобрено",
            "Completed" => "Завершено",
            "Cancelled" => "Отменено"
        };
    }
}
