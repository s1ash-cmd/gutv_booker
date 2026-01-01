using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace gutv_booker.Services.Telegram;

public class TelegramMenuService
{
    private readonly ITelegramBotClient _botClient;

    public TelegramMenuService(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task SendMainMenu(long userId)
    {
        var markup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "👤 Профиль", "📅 Мои бронирования" },
            new KeyboardButton[] { "ℹ️ Помощь" }
        })
        {
            ResizeKeyboard = true
        };

        const string text = "<b>Главное меню</b>\n\nВыберите действие:";

        await _botClient.SendMessage(
            userId,
            text,
            parseMode: ParseMode.Html,
            replyMarkup: markup
        );
    }
}
