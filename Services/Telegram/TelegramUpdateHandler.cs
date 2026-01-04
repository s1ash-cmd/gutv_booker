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
        _commands = RegisterCommands();
    }

    private Dictionary<string, Type> RegisterCommands()
    {
        var commands = new Dictionary<string, Type>();

        using var scope = _serviceProvider.CreateScope();

        var commandTypes = new List<Type>
        {
            typeof(StartCommand),
            typeof(LinkCommand),
            typeof(ProfileCommand),
            typeof(BookingCommand),
            typeof(HelpCommand),
            typeof(BackCommand)
        };

        foreach (var commandType in commandTypes)
        {
            try
            {
                var instance = (ICommand)ActivatorUtilities.CreateInstance(scope.ServiceProvider, commandType);
                commands[instance.Name] = commandType;
                _logger.LogInformation($"Зарегистрирована команда: {instance.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка регистрации команды {commandType.Name}");
            }
        }

        var filterButtons = new[]
        {
            "⏳ Ожидают",
            "✅ Одобренные",
            "🏁 Завершенные",
            "❌ Отмененные",
            "📋 Все бронирования"
        };

        foreach (var button in filterButtons)
        {
            commands[button] = typeof(BookingFilterCommand);
            _logger.LogInformation($"Зарегистрирована команда фильтра: {button}");
        }

        return commands;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.Text is not { } messageText)
            return;

        var chatId = update.Message.Chat.Id;
        var username = update.Message.From?.Username ?? "Unknown";

        _logger.LogInformation($"Получено от @{username} (ChatId: {chatId}): {messageText}");

        await UpdateUsername(chatId, username);

        var commandKey = messageText.Split(' ')[0];

        if (_commands.TryGetValue(messageText, out var commandType) ||
            _commands.TryGetValue(commandKey, out commandType))
        {
            await ExecuteCommand(commandType, botClient, update.Message, cancellationToken);
        }
        else
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: "❓ Неизвестная команда.\nДля вызова меню используйте /start",
                cancellationToken: cancellationToken);
        }
    }

    private async Task UpdateUsername(long chatId, string username)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UserService>();
            await userService.UpdateTelegramUsername(chatId, username == "Unknown" ? null : username);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Не удалось обновить username для ChatId: {chatId}");
        }
    }

    private async Task ExecuteCommand(Type commandType, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();

            ICommand commandInstance;

            if (commandType == typeof(BookingFilterCommand))
            {
                var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();

                var (status, displayName) = message.Text switch
                {
                    "⏳ Ожидают" => ("pending", "⏳ Ожидают"),
                    "✅ Одобренные" => ("approved", "✅ Одобренные"),
                    "🏁 Завершенные" => ("completed", "🏁 Завершенные"),
                    "❌ Отмененные" => ("cancelled", "❌ Отмененные"),
                    "📋 Все бронирования" => ("all", "📋 Все бронирования")
                };

                commandInstance = new BookingFilterCommand(bookingService, userService, status, displayName);
            }
            else
            {
                commandInstance = (ICommand)ActivatorUtilities.CreateInstance(scope.ServiceProvider, commandType);
            }

            await commandInstance.ExecuteAsync(botClient, message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка выполнения команды {commandType.Name}");

            await botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Произошла ошибка при выполнении команды",
                cancellationToken: cancellationToken);
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ошибка Telegram бота");
        return Task.CompletedTask;
    }
}