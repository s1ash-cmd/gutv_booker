using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace gutv_booker.Services.Telegram.Commands;

public class BookingCommand : ICommand
{
    private readonly UserService _userService;

    public BookingCommand(UserService userService)
    {
        _userService = userService;
    }

    public string Name => "📆 Мои бронирования";

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByTelegramChatId(message.Chat.Id);

        if (user == null)
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Пользователь не зарегистрирован.\n" +
                      "Используйте /link для привязки аккаунта.",
                cancellationToken: cancellationToken);
            return;
        }

        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "⏳ Ожидают", "✅ Одобренные" },
            new KeyboardButton[] { "🏁 Завершенные", "❌ Отмененные" },
            new KeyboardButton[] { "📋 Все бронирования" },
            new KeyboardButton[] { "« Назад в меню" }
        })
        {
            ResizeKeyboard = true
        };

        await botClient.SendMessage(
            chatId: message.Chat.Id,
            text: "📆 <b>Мои бронирования</b>\n\nВыберите категорию:",
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
}