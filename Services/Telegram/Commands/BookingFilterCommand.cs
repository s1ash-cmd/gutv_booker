using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text;
using gutv_booker.Models;

namespace gutv_booker.Services.Telegram.Commands;

public class BookingFilterCommand : ICommand
{
    private readonly BookingService _bookingService;
    private readonly UserService _userService;
    private readonly string _status;

    public BookingFilterCommand(BookingService bookingService, UserService userService, string status, string displayName)
    {
        _bookingService = bookingService;
        _userService = userService;
        _status = status;
        Name = displayName;
    }

    public string Name { get; }

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByTelegramChatId(message.Chat.Id);
        if (user == null) return;

        List<BookingResponseDto> bookings;
        try
        {
            var allBookings = await _bookingService.GetBookingsByUser(user.Id);

            if (_status == "all")
            {
                bookings = allBookings;
            }
            else
            {
                bookings = allBookings.Where(b => b.Status.ToLower() == _status.ToLower()).ToList();
            }

            if (!bookings.Any())
            {
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"❌ Нет бронирований со статусом <b>{GetStatusName(_status)}</b>",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
                return;
            }
        }
        catch (KeyNotFoundException)
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "📆 У вас пока нет бронирований",
                cancellationToken: cancellationToken);
            return;
        }

        var response = new StringBuilder();
        response.AppendLine($"📆 <b>{GetStatusName(_status)}</b>\n");

        foreach (var booking in bookings)
        {
            response.AppendLine($"🔹 <b>ID: {booking.Id}</b>");
            response.AppendLine($"   {GetStatusEmoji(booking.Status)} {GetStatusNameByString(booking.Status)}");
            response.AppendLine($"   📅 {booking.StartTime:dd.MM.yyyy HH:mm} - {booking.EndTime:dd.MM.yyyy HH:mm}");
            response.AppendLine($"   📝 {booking.Reason}");

            if (booking.EquipmentModelIds.Any())
            {
                response.AppendLine("   📦 Оборудование:");
                foreach (var item in booking.EquipmentModelIds)
                {
                    response.AppendLine($"      • {item.ModelName} ({item.InventoryNumber})");
                }
            }

            if (!string.IsNullOrEmpty(booking.Comment))
                response.AppendLine($"   💭 {booking.Comment}");

            if (!string.IsNullOrEmpty(booking.AdminComment))
                response.AppendLine($"   💬 Админ: {booking.AdminComment}");

            response.AppendLine();
        }

        var text = response.ToString();
        if (text.Length > 4000)
        {
            text = text.Substring(0, 4000) + "\n\n... (показаны первые бронирования)";
        }

        await botClient.SendMessage(
            chatId: message.Chat.Id,
            text: text,
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
    }

    private static string GetStatusEmoji(string status)
    {
        return status switch
        {
            "Pending" => "⏳",
            "Approved" => "✅",
            "Completed" => "🏁",
            "Cancelled" => "❌"
        };
    }

    private static string GetStatusNameByString(string status)
    {
        return status switch
        {
            "Pending" => "Ожидает",
            "Approved" => "Одобрено",
            "Completed" => "Завершено",
            "Cancelled" => "Отменено"
        };
    }

    private static string GetStatusName(string status)
    {
        return status.ToLower() switch
        {
            "pending" => "Ожидают подтверждения",
            "approved" => "Одобренные",
            "completed" => "Завершенные",
            "cancelled" => "Отмененные",
            "all" => "Все бронирования"
        };
    }
}
