using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace gutv_booker.Services.Telegram.Commands;

public class BackCommand : ICommand
{
    private readonly UserService _userService;

    public BackCommand(UserService userService)
    {
        _userService = userService;
    }

    public string Name => "« Назад в меню";

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByTelegramChatId(message.Chat.Id);

        if (user == null)
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Пользователь не найден",
                cancellationToken: cancellationToken);
            return;
        }

        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "👤 Профиль", "📆 Мои бронирования" },
            new KeyboardButton[] { "ℹ️ Помощь" }
        })
        {
            ResizeKeyboard = true
        };

        await botClient.SendMessage(
            chatId: message.Chat.Id,
            text: $"👋 Главное меню\n\nВыберите действие:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
}