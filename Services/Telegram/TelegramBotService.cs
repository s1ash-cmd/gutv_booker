using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace gutv_booker.Services.Telegram;

public class TelegramBotService : BackgroundService
{
    private readonly ILogger<TelegramBotService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITelegramBotClient _botClient;

    public TelegramBotService(
        ILogger<TelegramBotService> logger,
        IServiceProvider serviceProvider,
        ITelegramBotClient botClient)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _botClient = botClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var me = await _botClient.GetMe(stoppingToken);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandlePollingErrorAsync,
            receiverOptions,
            stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<TelegramUpdateHandler>();
            await handler.HandleUpdateAsync(botClient, update, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обработки обновления");
        }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ошибка при получении обновлений от Telegram");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Остановка бота");
        await base.StopAsync(cancellationToken);
    }
}
