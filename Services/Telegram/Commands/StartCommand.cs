using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace gutv_booker.Services.Telegram.Commands;

public class StartCommand : ICommand
{
    private readonly UserService _userService;
    private readonly ILogger<StartCommand> _logger;

    public StartCommand(UserService userService, ILogger<StartCommand> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public string Name => "/start";

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var username = message.From?.Username;

        var parts = message.Text?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string? startParameter = parts?.Length > 1 ? parts[1] : null;

        if (!string.IsNullOrEmpty(startParameter) && startParameter.StartsWith("LINK_"))
        {
            var code = startParameter.Replace("LINK_", "");

            if (code.Length == 6 && code.All(char.IsDigit))
            {
                _logger.LogInformation($"Попытка автопривязки. ChatId: {chatId}, Code: {code}");

                try
                {
                    var linkedUser = await _userService.LinkTelegramByCode(code, chatId, username);

                    var keyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "👤 Профиль", "📆 Мои бронирования" },
                        new KeyboardButton[] { "ℹ️ Помощь" }
                    })
                    {
                        ResizeKeyboard = true
                    };

                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "✅ <b>Telegram успешно привязан!</b>\n\n" +
                              $"👤 Имя: {linkedUser.Name}\n" +
                              $"📧 Логин: {linkedUser.Login}\n" +
                              $"💬 Telegram: @{username ?? "не установлен"}\n\n" +
                              "Теперь вы можете использовать все функции бота.\n" +
                              "Выберите действие из меню ниже:",
                        parseMode: ParseMode.Html,
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken);
                    return;
                }
                catch (KeyNotFoundException)
                {
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "❌ <b>Неверный код привязки</b>\n\n" +
                              "Код недействителен или устарел.\n" +
                              "Получите новый код в личном кабинете на сайте gutvbooker.ru",
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);
                    return;
                }
                catch (InvalidOperationException ex)
                {
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: $"❌ {ex.Message}",
                        cancellationToken: cancellationToken);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Ошибка автопривязки. ChatId: {chatId}, Code: {code}");

                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "❌ Произошла ошибка при привязке аккаунта.\n" +
                              "Попробуйте вручную: /link КОД",
                        cancellationToken: cancellationToken);
                    return;
                }
            }
        }

        var user = await _userService.GetUserByTelegramChatId(chatId);

        if (user == null)
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: "<b>👋 Добро пожаловать в GUtv Booker!</b>\n\n" +
                      "Для использования бота необходимо привязать ваш аккаунт:\n\n" +
                      "1️⃣ Зарегистрируйтесь на сайте gutvbooker.ru\n" +
                      "2️⃣ В личном кабинете нажмите 'Привязать Telegram'\n" +
                      "3️⃣ Нажмите на ссылку или скопируйте код\n" +
                      "4️⃣ Отправьте код сюда: /link КОД\n\n" +
                      $"💬 Ваш Telegram: @{username ?? "не установлен"}",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
            return;
        }

        var keyboard2 = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "👤 Профиль", "📆 Мои бронирования" },
            new KeyboardButton[] { "ℹ️ Помощь" }
        })
        {
            ResizeKeyboard = true
        };

        await botClient.SendMessage(
            chatId: chatId,
            text: $"👋 Здравствуйте, {user.Name}!\n\nВыберите действие:",
            replyMarkup: keyboard2,
            cancellationToken: cancellationToken);
    }
}
