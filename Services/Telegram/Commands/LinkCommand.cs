using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UserModel = gutv_booker.Models.User;

namespace gutv_booker.Services.Telegram.Commands;

public class LinkCommand : ICommand
{
    private readonly UserService _userService;
    private readonly ILogger<LinkCommand> _logger;

    public LinkCommand(UserService userService, ILogger<LinkCommand> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public string Name => "/link";

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var parts = message.Text?.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts == null || parts.Length != 2)
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Неверный формат команды.\n\n" +
                      "Используйте: <code>/link КОД</code>\n\n" +
                      "Код можно получить в личном кабинете на сайте.",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
            return;
        }

        var code = parts[1];

        if (code.Length != 6 || !code.All(char.IsDigit))
        {
            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Код должен состоять из 6 цифр.\n\n" +
                      "Получите новый код в личном кабинете.",
                cancellationToken: cancellationToken);
            return;
        }

        var chatId = message.Chat.Id;
        var username = message.From?.Username;

        try
        {
            var user = await _userService.LinkTelegramByCode(code, chatId, username);

            _logger.LogInformation($"Telegram аккаунт {username} (ChatId: {chatId}) привязан к пользователю {user.Login}");

            await botClient.SendMessage(
                chatId: chatId,
                text: "✅ <b>Telegram успешно привязан!</b>\n\n" +
                      $"Имя: {user.Name}\n" +
                      $"Логин: {user.Login}\n" +
                      $"Telegram: @{username ?? "не установлен"}\n" +
                      $"Роль: {GetRole(user.Role)} \n\n" +
                      "Теперь вы можете использовать все функции бота.\n" +
                      "Используйте /start для вызова меню.",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: "❌ <b>Неверный код привязки</b>\n\n" +
                      "Проверьте код или сгенерируйте новый в личном кабинете.",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: $"❌ {ex.Message}",
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при привязке аккаунта. ChatId: {chatId}, Code: {code}");

            await botClient.SendMessage(
                chatId: chatId,
                text: "❌ Произошла ошибка при привязке аккаунта.\n" +
                      "Попробуйте позже или обратитесь к администратору.",
                cancellationToken: cancellationToken);
        }
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
