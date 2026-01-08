using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using gutv_booker.Services.Telegram.Commands;

namespace gutv_booker.Services.Telegram;

public class TelegramUpdateHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelegramUpdateHandler> _logger;
    private readonly Dictionary<string, Type> _commands;
    private readonly ConcurrentDictionary<long, (string action, int bookingId)> _pendingComments = new();

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
            var instance = (ICommand)ActivatorUtilities.CreateInstance(scope.ServiceProvider, commandType);
            commands[instance.Name] = commandType;
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
        }

        return commands;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is { } callbackQuery)
        {
            await HandleCallbackQuery(botClient, callbackQuery, cancellationToken);
            return;
        }

        if (update.Message?.Text is not { } messageText)
            return;

        var chatId = update.Message.Chat.Id;
        var username = update.Message.From?.Username ?? "Unknown";

        _logger.LogInformation($"Получено от @{username} (ChatId: {chatId}): {messageText}");

        await UpdateUsername(chatId, username);

        if (_pendingComments.ContainsKey(chatId) && update.Message.ReplyToMessage != null)
        {
            await HandleCommentReply(botClient, update.Message, cancellationToken);
            return;
        }

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

    private async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        try
        {
            var data = callbackQuery.Data;
            var chatId = callbackQuery.Message!.Chat.Id;

            _logger.LogInformation($"Callback от ChatId: {chatId}, Data: {data}");

            if (data?.StartsWith("booking:") == true)
            {
                var parts = data.Split(':');
                if (parts.Length == 3)
                {
                    var action = parts[1];
                    var bookingId = int.Parse(parts[2]);

                    using var scope = _serviceProvider.CreateScope();
                    var userService = scope.ServiceProvider.GetRequiredService<UserService>();

                    var admin = await userService.GetUserByTelegramChatId(chatId);
                    if (admin?.Role != gutv_booker.Models.User.UserRole.Admin)
                    {
                        await botClient.AnswerCallbackQuery(
                            callbackQuery.Id,
                            "❌ У вас нет прав для этого действия",
                            showAlert: true,
                            cancellationToken: cancellationToken);
                        return;
                    }

                    _pendingComments[chatId] = (action, bookingId);

                    var actionText = action == "approve" ? "одобрения" : "отклонения";

                    await botClient.SendMessage(
                        chatId: chatId,
                        text: $"📝 Введите комментарий для {actionText} бронирования #{bookingId}\nили напишите \"-\" чтобы пропустить",
                        replyMarkup: new ForceReplyMarkup { Selective = true },
                        cancellationToken: cancellationToken);

                    await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обработки Callback Query");
            await botClient.AnswerCallbackQuery(
                callbackQuery.Id,
                "❌ Произошла ошибка",
                showAlert: true,
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleCommentReply(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;

        if (!_pendingComments.TryRemove(chatId, out var pendingData))
        {
            return;
        }

        var (action, bookingId) = pendingData;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();
            var userService = scope.ServiceProvider.GetRequiredService<UserService>();

            var admin = await userService.GetUserByTelegramChatId(chatId);

            var booking = await bookingService.GetBookingById(bookingId);
            if (booking.Status != "Pending")
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "❌ Это бронирование уже обработано",
                    cancellationToken: cancellationToken);
                return;
            }

            var comment = message.Text == "-" ? null : message.Text;
            var adminComment = comment != null ? $": {comment}" : null;

            if (action == "approve")
            {
                await bookingService.ApproveBooking(bookingId, adminComment);
                await botClient.SendMessage(
                    chatId: chatId,
                    text: $"✅ Бронирование #{bookingId} одобрено",
                    cancellationToken: cancellationToken);
            }
            else if (action == "reject")
            {
                await bookingService.CancelBooking(bookingId, admin.Id, true, adminComment);
                await botClient.SendMessage(
                    chatId: chatId,
                    text: $"❌ Бронирование #{bookingId} отклонено",
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обработки комментария");
            await botClient.SendMessage(
                chatId: chatId,
                text: "❌ Произошла ошибка",
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