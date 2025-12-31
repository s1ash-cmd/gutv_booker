using Telegram.Bot;
using Telegram.Bot.Types;

namespace gutv_booker.Services.Telegram.Commands;

public interface ICommand
{
    string Name { get; }
    Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
}