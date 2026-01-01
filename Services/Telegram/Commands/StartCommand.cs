using Telegram.Bot;
using Telegram.Bot.Types;

namespace gutv_booker.Services.Telegram.Commands;

public class StartCommand : ICommand
{
    private readonly TelegramMenuService _menuService;

    public StartCommand(TelegramMenuService menuService)
    {
        _menuService = menuService;
    }

    public string Name => "/start";

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await _menuService.SendMainMenu(message.Chat.Id);
    }
}
