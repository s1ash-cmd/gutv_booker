using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace gutv_booker.Services.Telegram;

public class TelegramBotHostedService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly TelegramUpdateHandler _updateHandler;

    public TelegramBotHostedService(
        ITelegramBotClient botClient,
        TelegramUpdateHandler updateHandler,
        ILogger<TelegramBotHostedService> logger)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _botClient.StartReceiving(
            _updateHandler.HandleUpdateAsync,
            _updateHandler.HandleErrorAsync,
            receiverOptions,
            stoppingToken
        );

        var me = await _botClient.GetMe(stoppingToken);
    }
}