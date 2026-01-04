using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text;
using UserModel = gutv_booker.Models.User;

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
        var user = await _userService.GetUserByTelegramChatId(message.Chat.Id);

        if (user == null)
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Пользователь не найден.\n" +
                      "Используйте /link для привязки аккаунта.",
                cancellationToken: cancellationToken);
            return;
        }

        var response = new StringBuilder();
        response.AppendLine("👤 <b>Ваш профиль:</b>\n");
        response.AppendLine($"<b>Имя:</b> <code>{user.Name}</code>");
        response.AppendLine($"<b>Логин:</b> <code>{user.Login}</code>");
        response.AppendLine($"<b>Роль:</b> <code>{GetRole(user.Role)}</code>");

        if (user.Role == UserModel.UserRole.Admin || user.Role == UserModel.UserRole.Ronin)
        {
            response.AppendLine($"<b>Разрешение на Ronin:</b> <code>Да</code>");
        }
        else
        {
            response.AppendLine($"<b>Разрешение на Ronin:</b> <code>Нет</code>");
        }


        if (user.Banned)
            response.AppendLine("\n🚫 <b>Аккаунт заблокирован</b>");

        await botClient.SendMessage(
            chatId: message.Chat.Id,
            text: response.ToString(),
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
    }

    private string GetRole(UserModel.UserRole role)
    {
        return role switch
        {
            UserModel.UserRole.Admin => "Администратор",
            UserModel.UserRole.Ronin => "Пользователь",
            UserModel.UserRole.Osnova => "Пользователь",
            UserModel.UserRole.User => "Пользователь"
        };
    }
}
