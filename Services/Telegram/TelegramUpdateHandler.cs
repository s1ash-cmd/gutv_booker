using Telegram.Bot;
using Telegram.Bot.Types;
using gutv_booker.Services.Telegram.Commands;

namespace gutv_booker.Services.Telegram;

public class TelegramUpdateHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelegramUpdateHandler> _logger;
    private readonly Dictionary<string, Type> _commands;

    public TelegramUpdateHandler(IServiceProvider serviceProvider, ILogger<TelegramUpdateHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        _commands = new Dictionary<string, Type>
        {
            { "/start", typeof(StartCommand) },
            { "👤 Профиль", typeof(ProfileCommand) }
            // { "📅 Мои бронирования", typeof(BookingsCommand) },
            // { "ℹ️ Помощь", typeof(HelpCommand) }
        };
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.Text is not { } messageText)
            return;

        var chatId = update.Message.Chat.Id;
        var username = update.Message.From?.Username ?? "Unknown";

        _logger.LogInformation($"Получено от @{username} chatId: {chatId}, текст: {messageText}");

        var command = messageText.Split(' ')[0];

        if (_commands.TryGetValue(messageText, out var commandType))
        {
            using var scope = _serviceProvider.CreateScope();
            var commandInstance = (ICommand)ActivatorUtilities.CreateInstance(scope.ServiceProvider, commandType);
            await commandInstance.ExecuteAsync(botClient, update.Message, cancellationToken);
        }
        else
        {
            await botClient.SendMessage(chatId, "❓Неизвестная команда. Используйте /start", cancellationToken: cancellationToken);
        }
    }


    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ошибка Telegram бота");
        return Task.CompletedTask;
    }
}