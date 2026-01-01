using Telegram.Bot;
using Telegram.Bot.Types;
using System.Text;

namespace gutv_booker.Services.Telegram.Commands;

public class ProfileCommand : ICommand
{
    private readonly UserService _userService;

    public ProfileCommand(UserService userService)
    {
        _userService = userService;
    }

    public string Name => "👤 Профиль";

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.GetUserByTelegramId(message.From.Username);

            if (user == null)
            {
                await botClient.SendMessage(message.Chat.Id, "Пользователь не зарегистрирован", cancellationToken: cancellationToken);
                return;
            }

            var response = new StringBuilder("👤 Ваш профиль:\n\n");
            response.AppendLine($"Имя: {user.Name}");
            response.AppendLine($"Логин: {user.Login}");
            response.AppendLine($"Telegram: @{user.TelegramId}");

            await botClient.SendMessage(message.Chat.Id, response.ToString(), cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            await botClient.SendMessage(message.Chat.Id, "Ошибка получения данных", cancellationToken: cancellationToken);
        }
    }
}