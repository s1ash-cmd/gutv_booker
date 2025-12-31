using Telegram.Bot;
using Telegram.Bot.Types;

namespace gutv_booker.Services.Telegram.Commands;

public class StartCommand : ICommand
{
    public string Name => "/start";

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var welcomeText = "👋 Добро пожаловать в GUtv Booker!\n\n" +
                          "Доступные команды:\n" +
                          "/equipment - Список оборудования\n" +
                          "/booking - Мои бронирования\n" +
                          "/help - Помощь";

        await botClient.SendMessage(
            chatId: message.Chat.Id,
            text: welcomeText,
            cancellationToken: cancellationToken
        );
    }
}